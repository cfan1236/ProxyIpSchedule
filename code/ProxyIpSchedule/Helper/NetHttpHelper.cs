using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Net;
using System.IO.Compression;
using System.Text.RegularExpressions;

namespace Common
{
    /// <summary>
    ///  简单http请求
    ///  author:王锐(Rick)
    /// </summary>
    public class NetHttpHelper
    {
        /// <summary>
        /// 简单HTTP POST请求
        /// </summary>
        /// <param name="url">请求url</param>
        /// <param name="postData">请求参数</param>
        /// <param name="status">http返回状态码</param>
        /// <param name="contentType"></param>
        /// <returns>返回数据</returns>
        public static string HttpPostRequest(string url, string postData, out int status, string contentType = "application/x-www-form-urlencoded", bool gbk = false)
        {
            Stream requestStream = null;
            string responseBody = string.Empty;
            status = 0;
            try
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
                if (!string.IsNullOrEmpty(postData))
                {
                    request.ContentType = contentType;
                    request.Method = "POST";
                    byte[] bytes = null;
                    if (gbk)
                    {
                        bytes = Encoding.GetEncoding("GBK").GetBytes(postData);
                    }
                    else
                    {
                        bytes = Encoding.UTF8.GetBytes(postData);

                    }
                    request.ContentLength = bytes.Length;
                    requestStream = request.GetRequestStream();
                    requestStream.Write(bytes, 0, bytes.Length);

                    HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                    MemoryStream _stream = new MemoryStream();
                    //判断压缩方式 gzip deflate
                    if (response.ContentEncoding != null && response.ContentEncoding.Equals("gzip", StringComparison.InvariantCultureIgnoreCase))
                    {
                        //解zip压缩方式
                        new GZipStream(response.GetResponseStream(), CompressionMode.Decompress).CopyTo(_stream, 10240);
                    }
                    else if (response.ContentEncoding != null && response.ContentEncoding.Equals("deflate", StringComparison.InvariantCultureIgnoreCase))
                    {
                        // //解Deflate压缩方式
                        new DeflateStream(response.GetResponseStream(), CompressionMode.Decompress).CopyTo(_stream, 10240);
                    }
                    else
                    {
                        //无压缩
                        response.GetResponseStream().CopyTo(_stream, 10240);

                    }
                    Encoding encoding = null;

                    byte[] byte_Response = _stream.ToArray();
                    if (encoding == null)
                    {
                        //获取源代码html
                        string temp_html = Encoding.Default.GetString(byte_Response, 0, byte_Response.Length);
                        Match meta = Regex.Match(temp_html, "<meta([^<]*)charset=([^<]*)[\"']", RegexOptions.IgnoreCase | RegexOptions.Multiline);
                        string charter = (meta.Groups.Count > 2) ? meta.Groups[2].Value : string.Empty;
                        charter = charter.Replace("\"", string.Empty).Replace("'", string.Empty).Replace(";", string.Empty);
                        //判断 html charter中是否有编码字符
                        if (charter.Length > 0)
                        {
                            charter = charter.ToLower().Replace("iso-8859-1", "gbk");
                            encoding = Encoding.GetEncoding(charter);
                        }
                        else
                        {
                            if (response.CharacterSet.ToLower().Trim() == "iso-8859-1")
                            {
                                encoding = Encoding.GetEncoding("gbk");
                            }
                            else
                            {
                                if (string.IsNullOrEmpty(response.CharacterSet.Trim()))
                                {
                                    encoding = Encoding.UTF8;
                                }
                                else
                                {
                                    encoding = Encoding.GetEncoding(response.CharacterSet);
                                }
                            }
                        }
                    }
                    responseBody = encoding.GetString(byte_Response);
                    status = (int)response.StatusCode;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                if (requestStream != null)
                {
                    requestStream.Close();
                }
            }

            return responseBody;
        }


        /// <summary>
        /// http异步请求
        /// </summary>
        /// <param name="url">url</param>
        /// <param name="reqMethod">请求方法 GET、POST</param>
        /// <param name="callback">回调函数</param>
        /// <param name="ob">回传对象</param>
        /// <param name="postData">post数据</param>
        public static void HttpAsyncRequest(string url, string reqMethod, AsyRequetCallback callback, object ob = null, string postData = "")
        {
            Stream requestStream = null;
            try
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);

                request.ContentType = "application/x-www-form-urlencoded";
                request.Method = reqMethod;
                if (reqMethod.ToUpper() == "POST")
                {
                    byte[] bytes = Encoding.UTF8.GetBytes(postData);
                    request.ContentLength = bytes.Length;
                    requestStream = request.GetRequestStream();
                    requestStream.Write(bytes, 0, bytes.Length);
                }
                //开始调用异步请求 
                //AsyResultTag 是自定义类 用于传递调用时信息 其中HttpWebRequest 是必须传递对象。因为回调后要获取HttpWebResponse 需要此对象
                request.BeginGetResponse(new AsyncCallback(HttpCallback), new AsyResultTag() { obj = ob, callback = callback, req = request });


            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                if (requestStream != null)
                {
                    requestStream.Close();
                }
            }

        }

        /// <summary>
        /// http请求回调 由.net内部调用 参数必须为IAsyncResult
        /// </summary>
        /// <param name="asynchronousResult">http回调时回传对象</param>
        private static void HttpCallback(IAsyncResult asynchronousResult)
        {
            int statusCode = 0;
            string retString = "";
            AsyResultTag tag = new AsyResultTag();
            try
            {
                //获取请求时传递的对象
                tag = asynchronousResult.AsyncState as AsyResultTag;
                HttpWebRequest req = tag.req;
                //获取异步返回的http结果
                HttpWebResponse response = req.EndGetResponse(asynchronousResult) as HttpWebResponse;
                Stream myResponseStream = response.GetResponseStream();
                StreamReader myStreamReader = new StreamReader(myResponseStream, Encoding.GetEncoding("utf-8"));
                retString = myStreamReader.ReadToEnd();
                myStreamReader.Close();
                myResponseStream.Close();
                statusCode = ((int)response.StatusCode);

            }
            catch (WebException ex)
            {
                if ((HttpWebResponse)ex.Response != null)
                {
                    statusCode = ((int)((HttpWebResponse)ex.Response).StatusCode);
                }

                throw;
            }
            //调用外部回调
            tag.callback(tag.obj, retString, statusCode);

        }

        /// <summary>
        /// 简单HTTP Head请求
        /// </summary>
        /// <param name="url"></param>
        /// <param name="dicHead">头内容</param>
        /// <param name="status">http返回状态</param>
        /// <returns></returns>
        public static string HttpHeadRequest(string url, Dictionary<string, string> dicHead, out int status)
        {
            Stream requestStream = null;
            string retString = string.Empty;
            status = 0;
            try
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);

                if (dicHead.Count > 0)
                {
                    request.ContentType = "application/x-www-form-urlencoded";
                    request.Method = "HEAD";
                    foreach (var item in dicHead)
                    {
                        request.Headers.Add(item.Key, item.Value);
                    }
                    HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                    Stream myResponseStream = response.GetResponseStream();
                    StreamReader myStreamReader = new StreamReader(myResponseStream, Encoding.GetEncoding("utf-8"));
                    retString = myStreamReader.ReadToEnd();
                    myStreamReader.Close();
                    myResponseStream.Close();
                    status = (int)response.StatusCode;

                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                if (requestStream != null)
                {
                    requestStream.Close();
                }
            }

            return retString;
        }
        /// <summary>
        /// 简单HTTP GET请求
        /// </summary>
        /// <param name="url">请求地址</param>
        /// <param name="status">http返回状态码</param>
        /// <returns>返回值</returns>
        public static string HttpGetRequest(string url, out int status, int timeOut = 0, string proxyIp = "", int proxyPort = 0)
        {
            string responseBody = string.Empty;
            status = 0;
            try
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
                if (proxyIp != "" && proxyPort != 0)
                {
                    request.Proxy = new WebProxy(proxyIp, proxyPort);
                }
                if (timeOut != 0)
                {
                    request.Timeout = timeOut;
                }
                request.Method = "GET";
                request.ContentType = "text/html;charset=UTF-8";
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                MemoryStream _stream = new MemoryStream();
                //判断压缩方式 gzip deflate
                if (response.ContentEncoding != null && response.ContentEncoding.Equals("gzip", StringComparison.InvariantCultureIgnoreCase))
                {
                    //解zip压缩方式
                    new GZipStream(response.GetResponseStream(), CompressionMode.Decompress).CopyTo(_stream, 10240);
                }
                else if (response.ContentEncoding != null && response.ContentEncoding.Equals("deflate", StringComparison.InvariantCultureIgnoreCase))
                {
                    // //解Deflate压缩方式
                    new DeflateStream(response.GetResponseStream(), CompressionMode.Decompress).CopyTo(_stream, 10240);
                }
                else
                {
                    //无压缩
                    response.GetResponseStream().CopyTo(_stream, 10240);

                }
                Encoding encoding = null;

                byte[] byte_Response = _stream.ToArray();
                if (encoding == null)
                {
                    //获取源代码html
                    string temp_html = Encoding.Default.GetString(byte_Response, 0, byte_Response.Length);
                    Match meta = Regex.Match(temp_html, "<meta([^<]*)charset=([^<]*)[\"']", RegexOptions.IgnoreCase | RegexOptions.Multiline);
                    string charter = (meta.Groups.Count > 2) ? meta.Groups[2].Value : string.Empty;
                    charter = charter.Replace("\"", string.Empty).Replace("'", string.Empty).Replace(";", string.Empty);
                    //判断 html charter中是否有编码字符
                    if (charter.Length > 0)
                    {
                        charter = charter.ToLower().Replace("iso-8859-1", "gbk");
                        encoding = Encoding.GetEncoding(charter);
                    }
                    else
                    {
                        if (response.CharacterSet.ToLower().Trim() == "iso-8859-1")
                        {
                            encoding = Encoding.GetEncoding("gbk");
                        }
                        else
                        {
                            if (string.IsNullOrEmpty(response.CharacterSet.Trim()))
                            {
                                encoding = Encoding.UTF8;
                            }
                            else
                            {
                                encoding = Encoding.GetEncoding(response.CharacterSet);
                            }
                        }
                    }
                }
                responseBody = encoding.GetString(byte_Response);
                status = (int)response.StatusCode;
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {

            }
            return responseBody;
        }
    }

    /// <summary>
    /// 异步请求回调委托
    /// </summary>
    /// <param name="asyObj"></param>
    /// <param name="respStr"></param>
    /// <param name="statusCode"></param>
    public delegate void AsyRequetCallback(object asyObj, string respStr, int statusCode);

    /// <summary>
    /// 异步返回对象
    /// </summary>
    class AsyResultTag
    {
        /// <summary>
        /// 回传对象
        /// </summary>
        public object obj { get; set; }
        /// <summary>
        /// 当前httpRequest请求实例
        /// </summary>
        public HttpWebRequest req { get; set; }
        /// <summary>
        /// 回调函数委托
        /// </summary>
        public AsyRequetCallback callback { get; set; }



    }


}
