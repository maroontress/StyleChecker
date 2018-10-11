namespace Application
{
    using System.IO;

    public sealed class Code
    {
        public byte[] byteArray;
        public BinaryReader binaryReader;

        public void Field()
        {
            System.Action<int, int> _readFully = (_offset, _length) =>
            {
                while (_length > 0)
                {
                    var _size = binaryReader.Read(byteArray, _offset, _length);
                    if (_size == 0)
                    {
                        throw new System.IO.EndOfStreamException();
                    }
                    _offset += _size;
                    _length -= _size;
                }
            };
            _readFully(0, 10);
        }

        public void Local()
        {
            var stream = new MemoryStream();
            var reader = new BinaryReader(stream);
            var buffer = new byte[4];
            System.Action<int, int> _readFully = (_offset, _length) =>
            {
                while (_length > 0)
                {
                    var _size = reader.Read(buffer, _offset, _length);
                    if (_size == 0)
                    {
                        throw new System.IO.EndOfStreamException();
                    }
                    _offset += _size;
                    _length -= _size;
                }
            };
            _readFully(0, 4);
        }
    }
}
