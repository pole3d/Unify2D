#pragma once

#include <mono/metadata/object.h>

#include <string>
#include <vector>

class CSharpAssembly
{
private:
    std::string m_AssemblyPath;

    MonoAssembly* m_Assembly;
    MonoImage* m_Image;
    
public:
    CSharpAssembly(std::string AssemblyPath);

public:
    void Reload();

    [[nodiscard]] MonoClass* GetClass(const std::string& ClassName) const;

private:
    static MonoAssembly* LoadAssembly(const std::string& Path);
    static void ReadAssembly(std::vector<char>& AssemblyData, const std::string& Path);
};
