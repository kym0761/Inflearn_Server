using System;
using System.IO;
using System.Xml;

namespace PacketGenerator
{
    class Program
    {

        static string GenPackets;
        static ushort PacketID;
        static string PacketEnums;

        static void Main(string[] args)
        {
            XmlReaderSettings setting = new XmlReaderSettings()
            {
                //comment,공백 무시.
                IgnoreComments = true,
                IgnoreWhitespace = true
            };

            using (XmlReader r = XmlReader.Create("PDL.xml", setting))
            {
                r.MoveToContent();

                while (r.Read())
                {
                    if (r.Depth == 1 && r.NodeType == XmlNodeType.Element)
                    {
                        ParsePacket(r);
                    }

                    //Console.WriteLine(r.Name + " " +r["name"]);
                }
                string fileText = string.Format(PacketFormat.FileFormat, PacketEnums, GenPackets);
                File.WriteAllText("GenPackets.cs", fileText);
            }

        }


        public static void ParsePacket(XmlReader r)
        {
            if (r.NodeType == XmlNodeType.EndElement)
            {
                Console.WriteLine("End Element is Not Valid");
                return;
            }

            if (r.Name.ToLower() != "packet")
            {
                Console.WriteLine("Invalid Packet Node");
                return;
            }

            string packetName = r["name"];
            if (string.IsNullOrEmpty(packetName))
            {
                Console.WriteLine("packet without name");
                return;
            }

            Tuple<string, string, string> t = ParseMembers(r);

            GenPackets += string.Format(PacketFormat._PacketFormat
                , packetName, t.Item1, t.Item2, t.Item3);

            PacketEnums += string.Format(PacketFormat.PacketEnumFormat,packetName,++PacketID) + Environment.NewLine + "\t";
        }

        //{1} memberVariables
        //{2} 멤버 변수 Read
        //{3} 멤머 변수 write
        public static Tuple<string, string, string> ParseMembers(XmlReader r)
        {
            string packetName = r["name"];

            string memberCode = "";
            string readCode = "";
            string writeCode = "";

            int depth = r.Depth + 1;

            while (r.Read())
            {
                if (r.Depth != depth)
                {
                    break;
                }

                string memberName = r["name"];
                if (string.IsNullOrEmpty(memberName))
                {
                    Console.WriteLine("Member without name");
                    return null;
                }

                if (string.IsNullOrEmpty(memberCode) == false)
                {
                    memberCode += Environment.NewLine;
                }
                if (string.IsNullOrEmpty(readCode) == false)
                {
                    readCode += Environment.NewLine;
                }
                if (string.IsNullOrEmpty(writeCode) == false)
                {
                    writeCode += Environment.NewLine;
                }


                string memberType = r.Name.ToLower();
                switch (memberType)
                {
                    case "byte":
                    case "sbyte":
                        memberCode += string.Format(PacketFormat.MemberFormat, memberType, memberName);
                        readCode += string.Format(PacketFormat.ReadByteFormat, memberName, memberType);
                        writeCode += string.Format(PacketFormat.WriteByteFormat, memberName, memberType);
                        break;
                    case "bool":
                    case "short":
                    case "ushort":
                    case "int":
                    case "long":
                    case "float":
                    case "double":
                        memberCode += string.Format(PacketFormat.MemberFormat, memberType, memberName);
                        readCode += string.Format(PacketFormat.ReadFormat, memberName,ToMemberType(memberType), memberType);
                        writeCode += string.Format(PacketFormat.WriteFormat, memberName, memberType);
                        break;
                    case "string":
                        memberCode += string.Format(PacketFormat.MemberFormat, memberType, memberName);
                        readCode += string.Format(PacketFormat.ReadStringFormat, memberName);
                        writeCode += string.Format(PacketFormat.WriteStringFormat, memberName);
                        break;
                    case "list":
                        Tuple<string, string, string> t = ParseList(r);
                        memberCode += t.Item1;
                        readCode += t.Item2;
                        writeCode += t.Item3;
                        break;

                    default:
                        break;

                }


            }

            memberCode = memberCode.Replace("\n", "\n\t");
            readCode = readCode.Replace("\n", "\n\t\t");
            writeCode = writeCode.Replace("\n", "\n\t\t");
            return new Tuple<string, string, string>(memberCode, readCode, writeCode);
        }

        public static Tuple<string, string, string> ParseList(XmlReader r)
        {

            string listName = r["name"];
            if (string.IsNullOrEmpty(listName))
            {
                Console.WriteLine("list without name");
                return null;
            }

            Tuple<string, string, string> t = ParseMembers(r);

            string memberCode = string.Format(PacketFormat.MemberListFormat, FirstCharToUpper(listName), FirstCharToLower(listName), t.Item1, t.Item2, t.Item3);
            string readCode = string.Format(PacketFormat.ReadListFormat, FirstCharToUpper(listName), FirstCharToLower(listName));
            string writeCode = string.Format(PacketFormat.WriteListFormat, FirstCharToUpper(listName), FirstCharToLower(listName));

            return new Tuple<string, string, string>(memberCode, readCode, writeCode);
        
        }

        public static string ToMemberType(string memberType)
        {
            switch (memberType)
            {
                case "bool":
                    return "ToBoolean";
                //case "byte":
                case "short":
                    return "ToInt16";
                case "ushort":
                    return "ToUInt16";
                case "int":
                    return "ToInt32";
                case "long":
                    return "ToInt64";
                case "float":
                    return "ToSingle";
                case "double":
                    return "ToDouble";
                default:
                    return "";
            }
        }

        public static string FirstCharToUpper(string input)
        {
            if (string.IsNullOrEmpty(input))
            {
                return "";
            }

            return input[0].ToString().ToUpper() + input.Substring(1);

        }

        public static string FirstCharToLower(string input)
        {
            if (string.IsNullOrEmpty(input))
            {
                return "";
            }

            return input[0].ToString().ToLower() + input.Substring(1);

        }

        


    }
}
