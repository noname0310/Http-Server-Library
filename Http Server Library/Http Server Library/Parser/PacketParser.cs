using System;
using System.Text;

namespace HttpServerLibrary
{
    static class PacketParser
    {
        public static ParseResult HeaderParse(byte[] buffer)
        {
            ParseResult parseResult = new ParseResult();

            switch (buffer[0])
            {
                case (byte)'G':
                    parseResult.RequestType = RequestType.GET;
                    parseResult.ParameterRange.StartIndex = 5;
                    break;

                case (byte)'P':

                    if (buffer[1] == (byte)'O')
                    {
                        parseResult.RequestType = RequestType.POST;
                        parseResult.ParameterRange.StartIndex = 6;
                    }
                    else
                    {
                        parseResult.RequestType = RequestType.PUT;
                        parseResult.ParameterRange.StartIndex = 5;
                    }
                    break;

                case (byte)'D':
                    parseResult.RequestType = RequestType.DELETE;
                    parseResult.ParameterRange.StartIndex = 7;
                    break;

                default:
                    goto case (byte)'G';
            }

            int[] searchresult = SearchString(buffer, parseResult.ParameterRange.StartIndex, new string[3] { "\r\n", "\r\n\r\n", "\0"});
            parseResult.ParameterRange.EndIndex = searchresult[0] - 10;
            parseResult.ContentRange = new Range(searchresult[1] + 4, ((searchresult[2] != 0) ? searchresult[2] : buffer.Length) - 1);
            
            if (parseResult.RequestType != RequestType.GET)
            {
                int lengthheaderpos = SearchString(buffer, searchresult[0], searchresult[1], "Content-Length: ");
                if (lengthheaderpos != -1)
                {
                    int lengthendpos = SearchString(buffer, lengthheaderpos, searchresult[1], "\r\n");
                    byte[] ContentLengthByte = new byte[(lengthendpos - 1) - (lengthheaderpos + 16) + 1];
                    Buffer.BlockCopy(buffer, lengthheaderpos + 16, ContentLengthByte, 0, ContentLengthByte.Length);
                    parseResult.ContentLength = int.Parse(Encoding.ASCII.GetString(ContentLengthByte));
                    parseResult.ReceivedContentLength = parseResult.ContentRange.EndIndex - parseResult.ContentRange.StartIndex + 1;
                }
                else
                {
                    parseResult.ContentLength = 0;
                    parseResult.ReceivedContentLength = 0;
                }
            }
            else
            {
                parseResult.ContentLength = 0;
                parseResult.ReceivedContentLength = 0;
            }

            return parseResult;
        }

        public static int SearchString(byte[] buffer, int startIndex, string target)
        {
            int targetcharindex = 0;

            for (int i = startIndex; i < buffer.Length; i++)
            {
                if (buffer[i] == target[targetcharindex])
                {
                    targetcharindex++;
                    if (targetcharindex == target.Length)
                    {
                        return i - target.Length + 1;
                    }
                }
                else
                    targetcharindex = 0;
            }

            return -1;
        }

        public static int SearchString(byte[] buffer, int startIndex, int endIndex, string target)
        {
            int targetcharindex = 0;

            for (int i = startIndex; i <= endIndex; i++)
            {
                if (buffer[i] == target[targetcharindex])
                {
                    targetcharindex++;
                    if (targetcharindex == target.Length)
                    {
                        return i - target.Length + 1;
                    }
                }
                else
                    targetcharindex = 0;
            }

            return -1;
        }

        public static int[] SearchString(byte[] buffer, int startIndex, string[] targets)
        {
            if (targets.Length == 0)
                return null;

            int[] results = new int[targets.Length];

            int targetindex = 0;
            int targetcharindex = 0;

            for (int i = startIndex; i < buffer.Length; i++)
            {
                string currenttarget = targets[targetindex];

                if (buffer[i] == currenttarget[targetcharindex])
                {
                    if (targetcharindex == 0)
                        results[targetindex] = i;

                    targetcharindex++;
                    if (targetcharindex == currenttarget.Length)
                    {
                        if (targetindex == targets.Length - 1)
                            break;
                        targetindex++;
                        targetcharindex = 0;
                    }
                }
                else
                    targetcharindex = 0;
            }

            return results;
        }
    }

    struct ParseResult
    {
        public RequestType RequestType;
        public int ContentLength;
        public int ReceivedContentLength;
        public Range ParameterRange;
        public Range ContentRange;
    }

    struct Range
    {
        public int StartIndex;
        public int EndIndex;

        public Range(int startIndex, int endIndex)
        {
            StartIndex = startIndex;
            EndIndex = endIndex;
        }
    }

    public enum RequestType
    {
        GET,
        POST,
        PUT,
        DELETE
    }
}
