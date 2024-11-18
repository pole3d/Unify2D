#include "Scripting.h"

#include "CSharpAssembly.h"

#include <mono/jit/jit.h>
#include <mono/metadata/assembly.h>

#include <iostream>
#include <stdarg.h>

void Hello()
{
    std::cout << "Hello from C++\n";
}

Scripting::Scripting()
{
    mono_set_assemblies_path("mono");

    m_RootDomain = mono_jit_init("Scripting");
    if (!m_RootDomain)
    {
        throw std::runtime_error("Failed to load Scripting domain");
    }

    CreateAppDomain();
}

Scripting::~Scripting()
{
    mono_jit_cleanup(m_RootDomain);
}

MonoDomain* Scripting::GetAppDomain() const
{
    return m_AppDomain;
}

std::vector<CSharpAssembly>& Scripting::GetAssemblies()
{
    return m_Assemblies;
}

void Scripting::CreateAppDomain()
{
    m_AppDomain = mono_domain_create_appdomain(const_cast<char*>("ScriptingDomain"), nullptr);

    mono_domain_set(m_AppDomain, true);
}

void* CreateEngine()
{
    return new Scripting();
}

void DeleteEngine(void* EnginePointer)
{
    delete static_cast<Scripting*>(EnginePointer);
}

void Reload(void* EnginePointer)
{
    Scripting& Engine = *static_cast<Scripting*>(EnginePointer);

    mono_domain_set(mono_get_root_domain(), false);
    mono_domain_unload(Engine.GetAppDomain());

    Engine.CreateAppDomain();

    for (CSharpAssembly& Assembly : Engine.GetAssemblies())
    {
        Assembly.Reload();
    }
}

void Open(void* EnginePointer, const char* AssemblyPath, int Length)
{
    Scripting& Engine = *static_cast<Scripting*>(EnginePointer);

    std::string Path(AssemblyPath, Length);

    Engine.GetAssemblies().push_back({ Path });
}

void* InstantiateClass(void* EnginePointer, const char* ClassName, int ClassNameLength, const char* NamespaceName, int NamespaceNameLength)
{
    std::string Class(ClassName, ClassNameLength);
    std::string Namespace(NamespaceName, NamespaceNameLength);

    Scripting& Engine = *static_cast<Scripting*>(EnginePointer);

    MonoClass* TheClass = nullptr;
    for (CSharpAssembly& Assembly : Engine.GetAssemblies())
    {
        if (TheClass = Assembly.GetClass(Class, Namespace))
        {
            break;
        }
    }

    if (!TheClass)
    {
        printf("Could not find class '%s::%s'\n", Namespace.c_str(), Class.c_str());
        return nullptr;
    }

    MonoObject* Object = mono_object_new(Engine.GetAppDomain(), TheClass);
    mono_runtime_object_init(Object);

    return Object;
}

void CallMethod(void* EnginePointer, void* ObjectPointer, const char* MethodName, int MethodNameLength, void* Parameter)
{
    Scripting& Engine = *static_cast<Scripting*>(EnginePointer);
    MonoObject* Object = static_cast<MonoObject*>(ObjectPointer);

    std::string TheMethod(MethodName, MethodNameLength);
    MonoMethod* Method = mono_class_get_method_from_name(mono_object_get_class(Object), TheMethod.c_str(), 0);

    std::vector<void*> Parameters { Parameter };
 
    MonoObject* Exception{};
    mono_runtime_invoke(Method, Object, Parameters.data(), &Exception);
}

void* GetComponents(void* EnginePointer, int& Count)
{
    Scripting& Engine = *static_cast<Scripting*>(EnginePointer);

    MonoClass* ComponentClass = nullptr;
    for (CSharpAssembly& Assembly : Engine.GetAssemblies())
    {
        if (ComponentClass = Assembly.GetClass("Component", "Unify2D.Core"))
        {
            break;
        }
    }
    
    if (!ComponentClass)
    {
        printf("Could not find class 'Unify2D.Core.Component'\n");
        return nullptr;
    }
    
    std::vector<ClassType*> Classes;

    CSharpAssembly& Assembly = Engine.GetAssemblies()[1];
    MonoImage* Image = Assembly.GetImage();

    const MonoTableInfo* TypeDefinitionTable = mono_image_get_table_info(Image, MONO_TABLE_TYPEDEF);
    int TypeCount = mono_table_info_get_rows(TypeDefinitionTable);

    for (int i = 0; i < TypeCount; ++i)
    {
        uint32_t Columns[MONO_TYPEDEF_SIZE];
        mono_metadata_decode_row(TypeDefinitionTable, i, Columns, MONO_TYPEDEF_SIZE);

        const char* TypeName = mono_metadata_string_heap(Image, Columns[MONO_TYPEDEF_NAME]);
        const char* TypeNamespace = mono_metadata_string_heap(Image, Columns[MONO_TYPEDEF_NAMESPACE]);
        
        MonoClass* Class = Assembly.GetClass(TypeName, TypeNamespace);
        if (!Class)
        {
            continue;
        }
        
        if (!mono_class_is_subclass_of(Class, ComponentClass, false))
        {
            continue;
        }

        Classes.push_back(new ClassType{
            TypeName,
            TypeNamespace
        });
    }

    ClassType** Types = new ClassType*[Classes.size()];
    for (std::size_t i = 0; i < Classes.size(); ++i)
    {
        Types[i] = Classes[i];
    }

    Count = Classes.size();
    
    return reinterpret_cast<void*>(Types);

    /*
    MonoImage* image = mono_assembly_get_image(duo_assembly.get_assembly());
    const MonoTableInfo* typeDefinitionsTable = mono_image_get_table_info(image, MONO_TABLE_TYPEDEF);
    int32_t numTypes = mono_table_info_get_rows(typeDefinitionsTable);

    for (int32_t i = 0; i < numTypes; i++)
    {
        uint32_t cols[MONO_TYPEDEF_SIZE];
        mono_metadata_decode_row(typeDefinitionsTable, i, cols, MONO_TYPEDEF_SIZE);
            
        const char* name = mono_metadata_string_heap(image, cols[MONO_TYPEDEF_NAME]);
        duo::clazz clazz = duo_assembly[name];

        MonoClass* parent = mono_class_get_parent(clazz.get_class());
        if (!parent) {
            continue;
        }

        const char* parent_name = mono_class_get_name(parent);
        if (strcmp(parent_name, "MonoBehaviour")) {
            continue;
        }

        classes.push_back(duo_assembly[name]);
    }
        
    return classes;
    */
}
