using System.Text;

    public static class EmbeddedResourceReader
    {
        /// <summary>
        /// Get a stream of embedded resource by name
        /// </summary>
        /// <param name="name">Name of embedded resource without any namespaces. Just the way it written in your csproj file</param>
        public static Stream? ReadStream(string name)
        {
            var assembly = System.Reflection.Assembly.GetEntryAssembly();
            var resourceName = assembly?.GetManifestResourceNames()?.Single(n => n.EndsWith(name));
            var resourceStream = assembly?.
                GetManifestResourceStream(resourceName ?? "");
            return resourceStream;
        }
        /// <summary>
        /// Get a byte array of embedded resource by name
        /// </summary>
        /// <param name="name">Name of embedded resource without any namespaces. Just the way it written in your csproj file</param>
        public static byte[] ReadBytes(string name){
            using var stream = ReadStream(name);
            var bytes = ReadFully(stream ?? Stream.Null);
            return bytes;
        }

        /// <summary>
        /// Get a string representation of embedded resource by name
        /// </summary>
        /// <param name="name">Name of embedded resource without any namespaces. Just the way it written in your csproj file</param>
        /// <param name="encoding">What encoding to use.</param>
        public static string ReadString(string name,Encoding encoding)
        {
            var bytes = ReadBytes(name);
            return encoding.GetString(bytes);
        }
        /// <summary>
        /// Reads all bytes from <see cref="Stream"/>
        /// </summary>
        static byte[] ReadFully(Stream input)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                input.CopyTo(ms);
                return ms.ToArray();
            }
        }
    }