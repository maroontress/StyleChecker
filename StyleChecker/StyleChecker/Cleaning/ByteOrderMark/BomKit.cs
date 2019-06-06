namespace StyleChecker.Cleaning.ByteOrderMark
{
    using System;
    using System.Collections.Immutable;
    using System.IO;
    using System.Linq;

    /// <summary>
    /// Manipulates a BOM.
    /// </summary>
    public static class BomKit
    {
        private static readonly ImmutableArray<byte> Utf8ByteOrderMark
            = ImmutableArray.Create<byte>(0xef, 0xbb, 0xbf);

        /// <summary>
        /// Gets whether the file of the specified path starts with UTF-8 BOM.
        /// </summary>
        /// <param name="path">
        /// The path of the file to be checked.
        /// </param>
        /// <returns>
        /// <c>true</c> if the file is readable and it starts with UTF-8 BOM,
        /// <c>false</c> otherwise.
        /// </returns>
        public static bool StartsWithBom(string path)
        {
            try
            {
                return StartsWithUtf8Bom(path);
            }
            catch (EndOfStreamException)
            {
                return false;
            }
            catch (DirectoryNotFoundException)
            {
                return false;
            }
            catch (FileNotFoundException)
            {
                return false;
            }
            catch (IOException)
            {
                return false;
            }
            catch (UnauthorizedAccessException)
            {
                return false;
            }
        }

        private static bool StartsWithUtf8Bom(string path)
        {
            static void ReadFully(Stream s, byte[] a, int o, int n)
            {
                var offset = o;
                var length = n;
                while (length > 0)
                {
                    var size = s.Read(a, offset, length);
                    if (size == 0)
                    {
                        throw new EndOfStreamException();
                    }
                    offset += size;
                    length -= size;
                }
            }

            var array = new byte[Utf8ByteOrderMark.Length];
            using var stream = new FileStream(path, FileMode.Open);
            ReadFully(stream, array, 0, array.Length);
            return array.SequenceEqual(Utf8ByteOrderMark);
        }
    }
}
