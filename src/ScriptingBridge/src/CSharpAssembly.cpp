#include "CSharpAssembly.h"

#include <filesystem>
#include <fstream>
#include <stdexcept>
#include <mono/metadata/assembly.h>

CSharpAssembly::CSharpAssembly(std::string AssemblyPath) :
m_AssemblyPath(std::move(AssemblyPath)),
m_Assembly(LoadAssembly(m_AssemblyPath)),
m_Image(mono_assembly_get_image(m_Assembly))
{
}

void CSharpAssembly::Reload()
{
    m_Assembly = LoadAssembly(m_AssemblyPath);
    m_Image = mono_assembly_get_image(m_Assembly);
}

MonoClass* CSharpAssembly::GetClass(const std::string& NameSpace, const std::string& ClassName) const
{
    return mono_class_from_name(m_Image, NameSpace.c_str(), ClassName.c_str());
}

MonoImage* CSharpAssembly::GetImage() const
{
    return m_Image;
}

std::string CSharpAssembly::GetAssemblyPath() const
{
    return m_AssemblyPath;
}

MonoAssembly* CSharpAssembly::LoadAssembly(const std::string& Path)
{
    std::vector<char> AssemblyData;
    ReadAssembly(AssemblyData, Path);

    MonoImageOpenStatus Status;
    MonoImage* Image = mono_image_open_from_data_full(AssemblyData.data(), AssemblyData.size(), true, &Status, false);

    if (Status != MONO_IMAGE_OK)
    {
        const char* ErrorMessage = mono_image_strerror(Status);
        throw std::runtime_error(ErrorMessage);
    }

    MonoAssembly* Assembly = mono_assembly_load_from_full(Image, Path.c_str(), &Status, false);
    mono_image_close(Image);
    return Assembly;
}

void CSharpAssembly::ReadAssembly(std::vector<char>& AssemblyData, const std::string& Path)
{
    std::size_t Size = std::filesystem::file_size(Path);
    std::ifstream AssemblyStream(Path, std::ios::binary);
    AssemblyData.resize(Size);
    AssemblyStream.read(AssemblyData.data(), static_cast<long long>(Size));
}
