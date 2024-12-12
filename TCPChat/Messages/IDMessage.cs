using System;
using System.IO;
using TCPChat.Extensions;

namespace TCPChat.Messages
{
    // ReSharper disable once InconsistentNaming
    public class IDMessage : Message
    {
        public Method Method { get; private set; }
        public string SendData { get; private set; }
        
        public IDMessage(Method method, string id = "")
        {
            Method = method;
            PostCode = 5;

            if (Method == Method.Send) SendData = id;
        }

        public IDMessage(byte[] data)
        {
            Deserialize(data);
        }
        
        public override byte[] Serialize()
        {
            var methodData = Method.ToString().Serialize();
            byte[] data = null;
            
            switch (Method)
            {
                case Method.Get:
                {
                    data = new byte[sizeof(int) + methodData.Length];
                    using var stream = new MemoryStream(data);
                    using var writer = new BinaryWriter(stream);
                    
                    writer.Write(PostCode);
                    writer.Write(methodData);

                    break;
                }
                    
                case Method.Send:
                {
                    var idData = SendData.Serialize();
                    data = new byte[sizeof(int) + methodData.Length + idData.Length];
                    using var stream = new MemoryStream(data);
                    using var writer = new BinaryWriter(stream);
                    
                    writer.Write(PostCode);
                    writer.Write(methodData);
                    writer.Write(idData);

                    break;
                }
            }

            return data;
        }

        public override void Deserialize(byte[] data)
        {
            using var stream = new MemoryStream(data);
            using var reader = new BinaryReader(stream);

            var code = reader.ReadInt32();

            if (code != 5) return;
            PostCode = code;

            switch (PostCode)
            {
                case 5:
                {
                    Method = Enum.Parse<Method>(
                        data.CopyFrom(sizeof(int)).DeserializeStrings(1)[0]
                    );

                        if(Method == Method.Send)
                        {
                            var sendData = data.CopyFrom(sizeof(int) + Method.ToString().GetBytesDataSize());
                            SendData = sendData.DeserializeStrings(1)[0];
                        }

                    break;
                }

                case 11:
                {
                    var messageArgs = data.CopyFrom(sizeof(int)).DeserializeStrings(2);

                    Method = Enum.Parse<Method>(messageArgs[0]);
                    SendData = messageArgs[1];

                    break;
                }
            }
        }
    }
}