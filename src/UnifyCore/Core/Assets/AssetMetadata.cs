using System;
using System.IO;

namespace UnifyCore
{
    /// <summary>
    /// Gère l'identité persistante des assets à travers leurs fichiers compagnons <c>.meta</c>.
    /// Un asset est identifié par un GUID plutôt que par son nom ou son emplacement :
    /// renommer ou déplacer un script peut ainsi conserver les références à son composant
    /// dans les scènes et les prefabs, à condition de conserver le même fichier <c>.meta</c>.
    /// </summary>
    /// <remarks>
    /// Le fichier de métadonnées se trouve à côté de l'asset et porte son nom complet suivi
    /// de <c>.meta</c> : par exemple, <c>Player.cs.meta</c> accompagne <c>Player.cs</c>.
    /// Il contient uniquement le GUID au format texte. Cette classe centralise sa création,
    /// sa lecture, sa validation et son déplacement, afin que l'éditeur et l'index des assets
    /// appliquent les mêmes règles de conservation de l'identité.
    /// <para>
    /// Si le fichier manque, un nouveau GUID est créé. S'il existe, son GUID est réutilisé ;
    /// une valeur invalide ou vide provoque une erreur au lieu d'être remplacée silencieusement.
    /// Un fichier <c>.meta</c> perdu ne permet donc pas de retrouver l'identité précédente :
    /// il faut restaurer l'original pour conserver les références déjà enregistrées.
    /// </para>
    /// <para>
    /// Lors d'un déplacement de fichier effectué par l'éditeur, l'asset et son <c>.meta</c>
    /// sont déplacés ensemble. Une destination déjà occupée est refusée ; si le déplacement
    /// du <c>.meta</c> échoue, la classe tente de remettre l'asset à son emplacement initial.
    /// Un déplacement de dossier emporte naturellement les fichiers <c>.meta</c> qu'il contient.
    /// Pour un renommage effectué hors de l'éditeur, elle peut déplacer le <c>.meta</c> resté
    /// à l'ancien emplacement, sans écraser celui qui existerait déjà à destination.
    /// </para>
    /// <para>
    /// Cette classe s'occupe des fichiers et de leurs GUID. La correspondance entre GUID et
    /// classes de composants, ainsi que la migration des scènes et prefabs, sont assurées
    /// par les systèmes de compilation et de sérialisation qui utilisent ces identités.
    /// </para>
    /// </remarks>
    public static class AssetMetadata
    {
        public static string GetOrCreateGuid(string assetPath)
        {
            string path = assetPath + ".meta";
            if (!File.Exists(path))
            {
                try
                {
                    using var stream = new FileStream(path, FileMode.CreateNew, FileAccess.Write);
                    using var writer = new StreamWriter(stream);
                    writer.Write(Guid.NewGuid().ToString("D"));
                }
                catch (IOException) when (File.Exists(path)) { }
            }
            string text = File.ReadAllText(path).Trim();
            if (!Guid.TryParse(text, out var guid) || guid == Guid.Empty)
                throw new InvalidDataException($"Invalid asset GUID in '{path}'. Restore the original .meta instead of generating a new identity.");
            return guid.ToString("D");
        }

        public static void Move(string source, string destination)
        {
            if (Directory.Exists(source))
            {
                Directory.Move(source, destination);
                return;
            }
            GetOrCreateGuid(source);
            if (File.Exists(destination) || File.Exists(destination + ".meta"))
                throw new IOException($"The asset or its .meta already exists: {destination}");
            File.Move(source, destination);
            try { File.Move(source + ".meta", destination + ".meta"); }
            catch
            {
                File.Move(destination, source);
                throw;
            }
        }

        public static void FollowExternalRename(string source, string destination)
        {
            if (!File.Exists(destination) || !File.Exists(source + ".meta") ||
                File.Exists(destination + ".meta")) return;
            File.Move(source + ".meta", destination + ".meta");
        }
    }
}
