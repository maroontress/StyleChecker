namespace StyleChecker.Test.Refactoring.IneffectiveReadByte
{
    using System.IO;

    public sealed class Code
    {
        public byte[] byteArray;
        public byte[] ByteArrayProperty { get; private set; } = new byte[1];
        public BinaryReader binaryReader;
        public BinaryReader BinaryReaderProperty { get; set; }

        public void Field()
        {
            {
                System.Action<byte[], int, int> _readFully = (_array, _offset, _length) =>
                {
                    var _reader = binaryReader;
                    while (_length > 0)
                    {
                        var _size = _reader.Read(_array, _offset, _length);
                        if (_size == 0)
                        {
                            throw new System.IO.EndOfStreamException();
                        }
                        _offset += _size;
                        _length -= _size;
                    }
                };
                _readFully(byteArray, 0, 10);
            }
        }

        public void Local()
        {
            var stream = new MemoryStream();
            var reader = new BinaryReader(stream);
            var buffer = new byte[4];
            {
                System.Action<byte[], int, int> _readFully = (_array, _offset, _length) =>
                {
                    var _reader = reader;
                    while (_length > 0)
                    {
                        var _size = _reader.Read(_array, _offset, _length);
                        if (_size == 0)
                        {
                            throw new System.IO.EndOfStreamException();
                        }
                        _offset += _size;
                        _length -= _size;
                    }
                };
                _readFully(buffer, 0, 4);
            }
        }

        public void AutoProperty()
        {
            {
                System.Action<byte[], int, int> _readFully = (_array, _offset, _length) =>
                {
                    var _reader = BinaryReaderProperty;
                    while (_length > 0)
                    {
                        var _size = _reader.Read(_array, _offset, _length);
                        if (_size == 0)
                        {
                            throw new System.IO.EndOfStreamException();
                        }
                        _offset += _size;
                        _length -= _size;
                    }
                };
                _readFully(ByteArrayProperty, 0, 1);
            }
        }

        public void Parameter(byte[] array, BinaryReader reader)
        {
            {
                System.Action<byte[], int, int> _readFully = (_array, _offset, _length) =>
                {
                    var _reader = reader;
                    while (_length > 0)
                    {
                        var _size = _reader.Read(_array, _offset, _length);
                        if (_size == 0)
                        {
                            throw new System.IO.EndOfStreamException();
                        }
                        _offset += _size;
                        _length -= _size;
                    }
                };
                _readFully(array, 0, 1);
            }
        }
    }
}
