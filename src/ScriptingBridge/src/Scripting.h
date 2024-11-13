#pragma once

#include <mono/metadata/object.h>
#include <mono/utils/mono-forward.h>

#include <vector>

#include <string>

#include "CSharpAssembly.h"
#include "Core/Core.h"

SCRIPTING_API void Hello();

class Scripting
{
private:
    MonoDomain* m_RootDomain;
    MonoDomain* m_AppDomain;

    std::vector<CSharpAssembly> m_Assemblies;

public:
    Scripting();
    ~Scripting();

    [[nodiscard]] MonoDomain* GetAppDomain() const;
    [[nodiscard]] std::vector<CSharpAssembly>& GetAssemblies();

public:
    void CreateAppDomain();
};

SCRIPTING_API void* CreateEngine();
SCRIPTING_API void DeleteEngine(void* EnginePointer);
SCRIPTING_API void Reload(void* EnginePointer);
SCRIPTING_API void Open(void* EnginePointer, const char* AssemblyPath, int Length);
SCRIPTING_API void InstantiateClass(void* EnginePointer, const char* ClassName, int Length);
