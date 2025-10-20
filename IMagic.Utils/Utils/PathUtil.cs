using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;


public static class PathUtil
{

    public static string GetPathToAbsoluteToSpecificRoot(string root, Enum path, params object[] args)
    {
        string relativePath = GetPath(path, args);
        relativePath = relativePath.Replace("~", "");
        string pathResolved = string.Format("{0}{1}", root, relativePath);
        return pathResolved;
    }

    public static string GetPathToAbsolute(Enum path, params object[] args)
    {
        string relativePath = GetPath(path, args);
        string pathResolved = ResolveUrlToAbsolute(relativePath);
        return pathResolved;
    }

    public static string GetPathForContentPage(Enum path, params object[] args)
    {
        return GetPath(path, false, args);
    }

    public static string GetPath(Enum path, params object[] args)
    {
        return GetPath(path, true, args);
    }
    //private static string GetPath(Enum path, bool encodeUrl, params object[] args)
    //{
    //    if (path is EnumPath)
    //    {
    //        EnumPath enupPath = (EnumPath)path;
    //        if (enupPath == EnumPath.EntityOriginalImages_FilePath)
    //        {
    //            return ConfigManager.FilePath_EntityOriginalImages;
    //        }
    //        else if (enupPath == EnumPath.GeneratedImages_FilePath)
    //        {
    //            return ConfigManager.FilePath_EntityGeneratedImages;
    //        }
    //        else if (enupPath == EnumPath.GeneratedImages_HttpPath)
    //        {
    //            return ConfigManager.HttpPath_EntityGeneratedImages;
    //        }
    //    }

    //    string url = path.EnumDescription();

    //    if (encodeUrl)
    //    {
    //        for (int i = 0; i < args.Length; i++)//canitise args
    //        {
    //            string s = args[i].ToString().Trim();
    //            s = s.Replace("&", "and");
    //            s = s.Replace(".", "-");
    //            s = s.Replace(" ", "-");
    //            s = s.RemoveAccent();
    //            s = s.StripNonAplhaNumeric();
    //            args[i] = s;
    //        }

    //        url = string.Format(url, args);//generate url

    //        url = url.Replace(" ", "-");
    //        url = url.RemoveMultipleDashes();
    //        url = url.Replace("-/", "/");
    //        url = url.ToLower();
    //    }


    //    return url;
    //}


    //public string SiteRootPath
    //{
    //    get
    //    {
    //        string output = HttpContext.Current.Request.Url.ToString();
    //        output = output.TrimEnd('?');
    //        if (HttpContext.Current.Request.Url.Query.Length > 0)
    //        {
    //            output = output.Replace(HttpContext.Current.Request.Url.Query, "");
    //        }
    //        return output.Replace(HttpContext.Current.Request.Path, "");
    //    }
    //}

    public static string ResolveUrlToAbsolute(string relativeUrl)
    {
        string output = ResolveServerUrl(relativeUrl);

        return output;
    }

  

    /// <summary>
    /// Returns a site relative HTTP path from a partial path starting out with a ~.
    /// Same syntax that ASP.Net internally supports but this method can be used
    /// outside of the Page framework.
    /// 
    /// Works like Control.ResolveUrl including support for ~ syntax
    /// but returns an absolute URL.
    /// </summary>
    /// <param name="originalUrl">Any Url including those starting with ~</param>
    /// <returns>relative url</returns>
    public static string ResolveUrl(string originalUrl)
    {
        if (string.IsNullOrEmpty(originalUrl))
        {
            return originalUrl;
        }

        // *** Absolute path - just return
        if (IsAbsolutePath(originalUrl))
        {
            return originalUrl;
        }

        // *** We don't start with the '~' -> we don't process the Url
        if (!originalUrl.StartsWith("~"))
        {
            return originalUrl;
        }

        // *** Fix up path for ~ root app dir directory
        // VirtualPathUtility blows up if there is a 
        // query string, so we have to account for this.
        int queryStringStartIndex = originalUrl.IndexOf('?');
        if (queryStringStartIndex != -1)
        {
            string queryString = originalUrl.Substring(queryStringStartIndex);
            string baseUrl = originalUrl.Substring(0, queryStringStartIndex);

            return string.Concat(
                VirtualPathUtility.ToAbsolute(baseUrl),
                queryString);
        }
        else
        {
            return VirtualPathUtility.ToAbsolute(originalUrl);
        }

    }

    /// <summary>
    /// This method returns a fully qualified absolute server Url which includes
    /// the protocol, server, port in addition to the server relative Url.
    /// 
    /// Works like Control.ResolveUrl including support for ~ syntax
    /// but returns an absolute URL.
    /// </summary>
    /// <param name="ServerUrl">Any Url, either App relative or fully qualified</param>
    /// <param name="forceHttps">if true forces the url to use https</param>
    /// <returns></returns>
    public static string ResolveServerUrl(string serverUrl, bool forceHttps)
    {
        if (string.IsNullOrEmpty(serverUrl))
        {
            return serverUrl;
        }

        // *** Is it already an absolute Url?
        if (IsAbsolutePath(serverUrl))
        {
            return serverUrl;
        }

        string newServerUrl = ResolveUrl(serverUrl);
        Uri result = new Uri(HttpContext.Current.Request.Url, newServerUrl);

        if (!forceHttps)
        {
            return result.ToString();
        }
        else
        {
            return ForceUriToHttps(result).ToString();
        }
    }

    /// <summary>
    /// This method returns a fully qualified absolute server Url which includes
    /// the protocol, server, port in addition to the server relative Url.
    /// 
    /// It work like Page.ResolveUrl, but adds these to the beginning.
    /// This method is useful for generating Urls for AJAX methods
    /// </summary>
    /// <param name="ServerUrl">Any Url, either App relative or fully qualified</param>
    /// <returns></returns>
    public static string ResolveServerUrl(string serverUrl)
    {
        return ResolveServerUrl(serverUrl, false);
    }

    /// <summary>
    /// Forces the Uri to use https
    /// </summary>
    private static Uri ForceUriToHttps(Uri uri)
    {
        // ** Re-write Url using builder.
        UriBuilder builder = new UriBuilder(uri);
        builder.Scheme = Uri.UriSchemeHttps;
        builder.Port = 443;

        return builder.Uri;
    }

    private static bool IsAbsolutePath(string originalUrl)
    {
        // *** Absolute path - just return
        int IndexOfSlashes = originalUrl.IndexOf("://");
        int IndexOfQuestionMarks = originalUrl.IndexOf("?");

        if (IndexOfSlashes <= -1 ||
             IndexOfQuestionMarks >= 0 &&
              (IndexOfQuestionMarks <= -1 || IndexOfQuestionMarks <= IndexOfSlashes)
            )
        {
            return false;
        }

        return true;
    }

}
