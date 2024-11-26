#include "Scripting.h"

#include "CSharpAssembly.h"

#include <mono/jit/jit.h>
#include <mono/metadata/assembly.h>

#include <iostream>
#include <cstdarg>

void Hello()
{
    std::cout << "Hello from C++\n";
}

Scripting::Scripting() :
m_ComponentClass(nullptr)
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

MonoClass* Scripting::GetComponentClass()
{
    if (!m_ComponentClass)
    {
        ForceFindComponentClass();
    }

    return m_ComponentClass;
}

MonoClass* Scripting::FindClassInAssemblies(const std::string& NamespaceName, const std::string& ClassName)
{
    MonoClass* Class = nullptr;
    for (const CSharpAssembly& Assembly : GetAssemblies())
    {
        if ((Class = Assembly.GetClass(NamespaceName, ClassName)))
        {
            break;
        }
    }

    return Class;
}

void Scripting::CreateAppDomain()
{
    m_AppDomain = mono_domain_create_appdomain(const_cast<char*>("ScriptingDomain"), nullptr);

    mono_domain_set(m_AppDomain, true);
}

void Scripting::ForceFindComponentClass()
{
    m_ComponentClass = FindClassInAssemblies("Unify2D.Core", "Component");
}

MonoMethod* Scripting::FindMethodInObjectRecursively(MonoObject* Object, const std::string& MethodName, bool HasParameter)
{
    MonoClass* ParentClass = mono_object_get_class(Object);
    MonoMethod* Method = nullptr;
    while (ParentClass && !Method)
    {
        Method = mono_class_get_method_from_name(ParentClass, MethodName.c_str(), HasParameter ? 1 : 0);
        ParentClass = mono_class_get_parent(ParentClass);
    }
    
    return Method;
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

    Engine.ForceFindComponentClass();
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

    MonoClass* TheClass = Engine.FindClassInAssemblies(Namespace, Class);
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
    MonoMethod* Method = Scripting::FindMethodInObjectRecursively(Object, TheMethod, Parameter ? true : false);
    if (!Method)
    {
        fprintf(stderr, "Could not find method \"%s\"\n", TheMethod.c_str());
        return;
    }
    
    std::vector<void*> Parameters;
    if (Parameter)
    {
        Parameters.push_back(Parameter);
    }
 
    MonoObject* Exception{};
    mono_runtime_invoke(Method, Object, Parameters.data(), &Exception);
}

void* GetComponents(void* EnginePointer, int& Count)
{
    Scripting& Engine = *static_cast<Scripting*>(EnginePointer);

    MonoClass* ComponentClass = Engine.GetComponentClass();
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
        
        MonoClass* Class = Assembly.GetClass(TypeNamespace, TypeName);
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

    Count = static_cast<int>(Classes.size());
    return reinterpret_cast<void*>(Types);
}
