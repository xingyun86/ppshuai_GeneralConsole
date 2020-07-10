using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;

namespace GeneralConsole
{
    [DataContract]
    class ChatMsg
    {
        [DataMember]
        public int ID { get; set; }

        [DataMember]
        public string Name { get; set; }
    }
    class SerializeUtilities
    {
        /// <summary>
        /// 序列化
        /// </summary>
        /// <param name="chatMsg"></param>
        /// <returns></returns>
        public static string ChatMsgSerializer(ChatMsg chatMsg)
        {
            DataContractJsonSerializer js = new DataContractJsonSerializer(typeof(ChatMsg));
            MemoryStream msObj = new MemoryStream();
            //将序列化之后的Json格式数据写入流中
            js.WriteObject(msObj, chatMsg);
            msObj.Position = 0;
            //从0这个位置开始读取流中的数据
            StreamReader sr = new StreamReader(msObj, System.Text.Encoding.UTF8);
            string json = sr.ReadToEnd();
            sr.Close();
            msObj.Close();
            return json;
        }
        /// <summary>
        /// 反序列化
        /// </summary>
        /// <param name="chatMsg"></param>
        /// <returns></returns>
        public static bool ChatMsgDeserializer(out ChatMsg model, string chatMsg)
        {
            try
            {
                var ms = new MemoryStream(System.Text.Encoding.Unicode.GetBytes(chatMsg));
                DataContractJsonSerializer deseralizer = new DataContractJsonSerializer(typeof(ChatMsg));
                model = (ChatMsg)deseralizer.ReadObject(ms);
            }
            catch(System.Exception e)
            {
                model = null;
                return false;
            }
            return true;
        }
    }
}
