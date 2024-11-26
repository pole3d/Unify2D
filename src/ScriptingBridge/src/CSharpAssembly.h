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

    [[nodiscard]] MonoClass* GetClass(const std::string& NameSpace, const std::string& ClassName) const;
    [[nodiscard]] MonoImage* GetImage() const;
    [[nodiscard]] std::string GetAssemblyPath() const;

private:
    static MonoAssembly* LoadAssembly(const std::string& Path);
    static void ReadAssembly(std::vector<char>& AssemblyData, const std::string& Path);
};
