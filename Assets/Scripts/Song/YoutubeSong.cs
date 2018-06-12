using System.IO;
//using MediaToolkit;
//using MediaToolkit.Model;
using UnityEngine;
using VideoLibrary;

[CreateAssetMenu]
public class YoutubeSong : Song
{
	public string YoutubeId;
	
	private const string YOUTUBE_PREFIX = "https://www.youtube.com/watch?v=";
	private const string FFMPEG_PATH = "E:\\Program Files (x86)\\ffmpeg-4.0\\bin\\ffmpeg.exe";
	
	//TODO: Download thumbnail https://i.ytimg.com/vi/dQw4w9WgXcQ/maxresdefault.jpg
	
	protected override void Resolve()
	{
		/*var youtube = YouTube.Default;
		var vid = youtube.GetVideo(YOUTUBE_PREFIX + YoutubeId);
		
		File.WriteAllBytes(TempFilepath, vid.GetBytes());

		var inputFile = new MediaFile {Filename = TempFilepath};
		var outputFile = new MediaFile {Filename = Filepath};

		using (var engine = new Engine(FFMPEG_PATH))
		{
			engine.GetMetadata(inputFile);
			engine.Convert(inputFile, outputFile);
		}
		
		File.Delete(TempFilepath);*/
	}
	
	private string TempFilepath => $"{Application.dataPath}/Temp/{FullName}.mp4";
}
