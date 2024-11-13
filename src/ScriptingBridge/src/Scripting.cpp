#include "Scripting.h"

#include <mono/jit/jit.h>
#include <mono/metadata/assembly.h>

#include <iostream>

#include "CSharpAssembly.h"

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

void InstantiateClass(void* EnginePointer, const char* ClassName, int Length)
{
    std::string Class(ClassName, Length);
    
    Scripting& Engine = *static_cast<Scripting*>(EnginePointer);

    MonoClass* TheClass = nullptr;
    for (CSharpAssembly& Assembly : Engine.GetAssemblies())
    {
        if (TheClass = Assembly.GetClass(Class))
        {
            break;
        }
    }

    if (!TheClass)
    {
        printf("Could not find class '%s'\n", ClassName);
        return;
    }
    
    MonoObject* Object = mono_object_new(Engine.GetAppDomain(), TheClass);
    mono_runtime_object_init(Object);
    
    MonoMethod* Method = mono_class_get_method_from_name(TheClass, "Greet", 1);
    std::vector<void*> Arguments;
    Arguments.push_back(mono_string_new(Engine.GetAppDomain(), "world"));
    
    MonoObject* Exception;
    mono_runtime_invoke(Method, Object, Arguments.data(), &Exception);
}
