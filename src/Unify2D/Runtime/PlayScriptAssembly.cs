using System;
using System.IO;
using System.Reflection;
using System.Runtime.Loader;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Unify2D.Runtime
{
    internal sealed class PlayScriptAssembly : ISerializationBinder, IDisposable
    {
        private AssemblyLoadContext _context;
        private Assembly _assembly;
        private readonly GuidTypeBinder _binder = new();
        internal Type[] Types => _assembly.GetTypes();

        public PlayScriptAssembly(byte[] image)
        {
            _context = new AssemblyLoadContext("Play-" + Guid.NewGuid(), isCollectible: true);
            using var stream = new MemoryStream(image, writable: false);
            _assembly = _context.LoadFromStream(stream);
            _binder.SetScriptAssembly(_assembly);
        }

        public Type BindToType(string assemblyName, string typeName)
        {
            if (Guid.TryParse(typeName, out _))
            {
                return _binder.BindToType(assemblyName, typeName);
            }

            string qualifiedName = string.IsNullOrEmpty(assemblyName) ? typeName : $"{typeName}, {assemblyName}";
            return Type.GetType(qualifiedName,
                name => name.Name == _assembly.GetName().Name ? _assembly : Assembly.Load(name),
                (assembly, name, ignoreCase) => (assembly ?? _assembly).GetType(name, false, ignoreCase),
                throwOnError: true);
        }

        public void BindToName(Type type, out string assemblyName, out string typeName)
        {
            _binder.BindToName(type, out assemblyName, out typeName);
        }

        public void Dispose()
        {
            _binder.SetScriptAssembly(null);
            _assembly = null;
            _context?.Unload();
            _context = null;
        }
    }
}
