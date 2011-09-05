﻿using System.Linq;
using System.Collections.Generic;
using System.IO;
using MediaServer.Utility;

namespace MediaServer.Media
{
	static class MimeTypeLookup
	{
	    private static readonly IDictionary<string, string> MimeTypes =
	        new Dictionary<string, string>
	            {
	                {"ai", "application/postscript"},
	                {"aif", "audio/x-aiff"},
	                {"aifc", "audio/x-aiff"},
	                {"aiff", "audio/x-aiff"},
	                {"asc", "text/plain"},
	                {"atom", "application/atom+xml"},
	                {"au", "audio/basic"},
	                {"avi", "video/x-msvideo"},
	                {"bcpio", "application/x-bcpio"},
	                {"bin", "application/octet-stream"},
	                {"bmp", "image/bmp"},
	                {"cdf", "application/x-netcdf"},
	                {"cgm", "image/cgm"},
	                {"class", "application/octet-stream"},
	                {"cpio", "application/x-cpio"},
	                {"cpt", "application/mac-compactpro"},
	                {"csh", "application/x-csh"},
	                {"css", "text/css"},
	                {"dcr", "application/x-director"},
	                {"dif", "video/x-dv"},
	                {"dir", "application/x-director"},
	                {"djv", "image/vnd.djvu"},
	                {"djvu", "image/vnd.djvu"},
	                {"dll", "application/octet-stream"},
	                {"dmg", "application/octet-stream"},
	                {"dms", "application/octet-stream"},
	                {"doc", "application/msword"},
	                {"dtd", "application/xml-dtd"},
	                {"dv", "video/x-dv"},
	                {"dvi", "application/x-dvi"},
	                {"dxr", "application/x-director"},
	                {"eps", "application/postscript"},
	                {"etx", "text/x-setext"},
	                {"exe", "application/octet-stream"},
	                {"ez", "application/andrew-inset"},
	                {"gif", "image/gif"},
	                {"gram", "application/srgs"},
	                {"grxml", "application/srgs+xml"},
	                {"gtar", "application/x-gtar"},
	                {"hdf", "application/x-hdf"},
	                {"hqx", "application/mac-binhex40"},
	                {"htm", "text/html"},
	                {"html", "text/html"},
	                {"ice", "x-conference/x-cooltalk"},
	                {"ico", "image/x-icon"},
	                {"ics", "text/calendar"},
	                {"ief", "image/ief"},
	                {"ifb", "text/calendar"},
	                {"iges", "model/iges"},
	                {"igs", "model/iges"},
	                {"jnlp", "application/x-java-jnlp-file"},
	                {"jp2", "image/jp2"},
	                {"jpe", "image/jpeg"},
	                {"jpeg", "image/jpeg"},
	                {"jpg", "image/jpeg"},
	                {"js", "application/x-javascript"},
	                {"kar", "audio/midi"},
	                {"latex", "application/x-latex"},
	                {"lha", "application/octet-stream"},
	                {"lzh", "application/octet-stream"},
	                {"m3u", "audio/x-mpegurl"},
	                {"m4a", "audio/mp4"},
	                //mp4a-latm 
	                {"m4b", "audio/mp4"},
	                {"m4p", "audio/mp4"},
	                {"m4u", "video/vnd.mpegurl"},
	                {"m4v", "video/mp4" /*"video/x-m4v"*/},
	                // changed to make PS3 happy.  didnt like x-m4v  I suspect some others will need changed too
	                {"mac", "image/x-macpaint"},
	                {"man", "application/x-troff-man"},
	                {"mathml", "application/mathml+xml"},
	                {"me", "application/x-troff-me"},
	                {"mesh", "model/mesh"},
	                {"mid", "audio/midi"},
	                {"midi", "audio/midi"},
	                {"mif", "application/vnd.mif"},
	                {"mov", "video/quicktime"},
	                {"movie", "video/x-sgi-movie"},
	                {"mp2", "audio/mpeg"},
	                {"mp3", "audio/mpeg"},
	                {"mp4", "video/mp4"},
	                {"mpe", "video/mpeg"},
	                {"mpeg", "video/mpeg"},
	                {"mpg", "video/mpeg"},
	                {"mpga", "audio/mpeg"},
	                {"ms", "application/x-troff-ms"},
	                {"msh", "model/mesh"},
	                {"mxu", "video/vnd.mpegurl"},
	                {"nc", "application/x-netcdf"},
	                {"oda", "application/oda"},
	                {"ogg", "application/ogg"},
	                {"pbm", "image/x-portable-bitmap"},
	                {"pct", "image/pict"},
	                {"pdb", "chemical/x-pdb"},
	                {"pdf", "application/pdf"},
	                {"pgm", "image/x-portable-graymap"},
	                {"pgn", "application/x-chess-pgn"},
	                {"pic", "image/pict"},
	                {"pict", "image/pict"},
	                {"png", "image/png"},
	                {"pnm", "image/x-portable-anymap"},
	                {"pnt", "image/x-macpaint"},
	                {"pntg", "image/x-macpaint"},
	                {"ppm", "image/x-portable-pixmap"},
	                {"ppt", "application/vnd.ms-powerpoint"},
	                {"ps", "application/postscript"},
	                {"qt", "video/quicktime"},
	                {"qti", "image/x-quicktime"},
	                {"qtif", "image/x-quicktime"},
	                {"ra", "audio/x-pn-realaudio"},
	                {"ram", "audio/x-pn-realaudio"},
	                {"ras", "image/x-cmu-raster"},
	                {"rdf", "application/rdf+xml"},
	                {"rgb", "image/x-rgb"},
	                {"rm", "application/vnd.rn-realmedia"},
	                {"roff", "application/x-troff"},
	                {"rtf", "text/rtf"},
	                {"rtx", "text/richtext"},
	                {"sgm", "text/sgml"},
	                {"sgml", "text/sgml"},
	                {"sh", "application/x-sh"},
	                {"shar", "application/x-shar"},
	                {"silo", "model/mesh"},
	                {"sit", "application/x-stuffit"},
	                {"skd", "application/x-koan"},
	                {"skm", "application/x-koan"},
	                {"skp", "application/x-koan"},
	                {"skt", "application/x-koan"},
	                {"smi", "application/smil"},
	                {"smil", "application/smil"},
	                {"snd", "audio/basic"},
	                {"so", "application/octet-stream"},
	                {"spl", "application/x-futuresplash"},
	                {"src", "application/x-wais-source"},
	                {"sv4cpio", "application/x-sv4cpio"},
	                {"sv4crc", "application/x-sv4crc"},
	                {"svg", "image/svg+xml"},
	                {"swf", "application/x-shockwave-flash"},
	                {"t", "application/x-troff"},
	                {"tar", "application/x-tar"},
	                {"tcl", "application/x-tcl"},
	                {"tex", "application/x-tex"},
	                {"texi", "application/x-texinfo"},
	                {"texinfo", "application/x-texinfo"},
	                {"tgz", "application/x-compressed"},
	                {"tif", "image/tiff"},
	                {"tiff", "image/tiff"},
	                {"tr", "application/x-troff"},
	                {"tsv", "text/tab-separated-values"},
	                {"txt", "text/plain"},
	                {"ustar", "application/x-ustar"},
	                {"vcd", "application/x-cdlink"},
	                {"vrml", "model/vrml"},
	                {"vxml", "application/voicexml+xml"},
	                {"wav", "audio/x-wav"},
	                {"wbmp", "image/vnd.wap.wbmp"},
	                {"wbmxl", "application/vnd.wap.wbxml"},
	                {"wml", "text/vnd.wap.wml"},
	                {"wmlc", "application/vnd.wap.wmlc"},
	                {"wmls", "text/vnd.wap.wmlscript"},
	                {"wmlsc", "application/vnd.wap.wmlscriptc"},
	                {"wrl", "model/vrml"},
	                {"xbm", "image/x-xbitmap"},
	                {"xht", "application/xhtml+xml"},
	                {"xhtml", "application/xhtml+xml"},
	                {"xls", "application/vnd.ms-excel"},
	                {"xml", "application/xml"},
	                {"xpm", "image/x-xpixmap"},
	                {"xsl", "application/xml"},
	                {"xslt", "application/xslt+xml"},
	                {"xul", "application/vnd.mozilla.xul+xml"},
	                {"xwd", "image/x-xwindowdump"},
	                {"xyz", "chemical/x-xyz"},
	                {"zip", "application/zip"},
	            };

		public static IEnumerable<KeyValuePair<string,string>> GetAllMappings()
		{
			return MimeTypes;
		}

		public static string GetMimeType(string filename)
		{
			var s = Path.GetExtension(filename);
			if (s != null)
			{
				var extension = s.Substring(1).ToLower();
				if (MimeTypes.ContainsKey(extension))
				{
					return MimeTypes[extension];
				}
				Logger.Instance.Warn("Didnt find mime type for {0}", Path.GetFileName(extension));
			}
			return "application/octet-stream";
		}

		public static IEnumerable<string> GetExtensionsForType(string type)
		{
			return MimeTypes.Where(item => item.Value == type).Select(item => "." + item.Key);
		}

	}
}
