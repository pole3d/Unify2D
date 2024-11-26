#pragma once

#include <mono/utils/mono-forward.h>

#include <vector>

#include "CSharpAssembly.h"
#include "Core/Core.h"

SCRIPTING_API void Hello();

struct ClassType
{
    const char* Name;
    const char* Namespace;
};

class Scripting
{
private:
    MonoDomain* m_RootDomain;
    MonoDomain* m_AppDomain;

    std::vector<CSharpAssembly> m_Assemblies;

    MonoClass* m_ComponentClass;

public:
    Scripting();
    ~Scripting();

    [[nodiscard]] MonoDomain* GetAppDomain() const;
    [[nodiscard]] std::vector<CSharpAssembly>& GetAssemblies();
    [[nodiscard]] MonoClass* GetComponentClass();

    [[nodiscard]] MonoClass* FindClassInAssemblies(const std::string& NamespaceName, const std::string& ClassName);

    void CreateAppDomain();
    void ForceFindComponentClass();

    [[nodiscard]] static MonoMethod* FindMethodInObjectRecursively(MonoObject* Object, const std::string& MethodName, bool HasParameter);
};

SCRIPTING_API void* CreateEngine();
SCRIPTING_API void DeleteEngine(void* EnginePointer);
SCRIPTING_API void Reload(void* EnginePointer);
SCRIPTING_API void Open(void* EnginePointer, const char* AssemblyPath, int Length);
SCRIPTING_API void* InstantiateClass(void* EnginePointer, const char* ClassName, int ClassNameLength, const char* NamespaceName, int NamespaceNameLength);
SCRIPTING_API void CallMethod(void* EnginePointer, void* ObjectPointer, const char* MethodName, int MethodNameLength, void* Parameter);
SCRIPTING_API void* GetComponents(void* EnginePointer, int& Count);
