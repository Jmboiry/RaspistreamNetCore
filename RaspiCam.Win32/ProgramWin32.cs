/********
This work is an implementation in C# Net.Core by Jean-Marie Boiry <jean-marie.boiry@live.fr> 
of Python modules from pycamera (https://picamera.readthedocs.io/en/release-1.13/)
so the following copyright that apply to this software
**/
/**************************************
Copyright 2013-2017 Dave Jones<dave@waveform.org.uk>

Redistribution and use in source and binary forms, with or without
modification, are permitted provided that the following conditions are met:

    * Redistributions of source code must retain the above copyright
	  notice, this list of conditions and the following disclaimer.

	* Redistributions in binary form must reproduce the above copyright
	  notice, this list of conditions and the following disclaimer in the
	  documentation and/or other materials provided with the distribution.

	* Neither the name of the copyright holder nor the

	  names of its contributors may be used to endorse or promote products

	  derived from this software without specific prior written permission.

THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS"
AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE
IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE
ARE DISCLAIMED.IN NO EVENT SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS BE
LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR
CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF
SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS
INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN
CONTRACT, STRICT LIABILITY, OR TORT(INCLUDING NEGLIGENCE OR OTHERWISE)
ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE
POSSIBILITY OF SUCH DAMAGE.

******************************/
using System;
using System.IO;
using System.Threading;
using PiCamera;

namespace RaspiCam
{
	class Program
	{
		void TestCapture()
		{
			using (Camera camera = new Camera())
			{
				//var stream = new MemoryStream();
				Console.WriteLine("About to change camera resolution");
				camera.Resolution = (1920, 1080);
				Console.WriteLine("Camera resolution changed");
				using (FileStream s = new FileStream("Aloha.mjpg", FileMode.OpenOrCreate))
				{
					camera.Capture((buffer => { s.Write(buffer.Data, 0, buffer.Length); return true; }), format: ImageFormat.jpeg, options: new string[] { "quality=50", "picture;750" });
				}
			}
		}

		void TestRecording(string option)
		{
			var f = "";
			if (option == "m")
				f = "mjpeg";
			else
				f = "h264";
			using (Camera camera = new Camera())
			{
				//var stream = new MemoryStream();
				camera.Resolution = (800, 600);
				Thread.Sleep(1000);
				using (FileStream s = new FileStream("Aloha.mp4", FileMode.Create))
				{
					camera.StartRecording(buffer => { s.Write(buffer.Data, 0, buffer.Data.Length); return true; }, format: ImageFormat.h264, resize: 1, splitterPort:1, options: new string[] { "profile=lustucru", "test"});
					camera.WaitRecording(30);
					camera.StopRecording();
				}
			}
		}

		//void TestMalVideo(string option)
		//{
		//	var camera = new MMalVideo();
		//	camera.CreateCameraComponent();
		//	//camera.CreatePreview();
		//	camera.CreateEncoderComponent();
		//	camera.FinishInit();
		//	using (FileStream s = new FileStream("Aloha.h264", FileMode.OpenOrCreate))
		//	{

		//		camera.Capture(s);
		//	}
		//}

		//void TestMalCamera(string option)
		//{
		//	var camera = new _MMalCamera();
		//	camera.CreateCameraComponent();
		//	camera.CreatePreview();
		//	camera.CreateEncoderComponent();
		//	camera.FinishInit();
		//	using (FileStream s = new FileStream("Aloha.jpg", FileMode.OpenOrCreate))
		//	{

		//		camera.Capture((e => s.Write(e, 0, e.Length)));
		//	}
		//}

		void TestParser()
		{

			//var file = "D:\\source\\repos\\NetCore\\RaspiCam\\Debug\\Aloha.h264";
			//var fs = new FileStream(file, FileMode.Open);
			//byte[] _buffer = new byte[fs.Length];
			//fs.Read(_buffer, 0, (int)fs.Length);

			//GF_MediaImporter import = new GF_MediaImporter();
			//import.dest = GF_ISOFile.gf_isom_create_movie("Aloha.mp4", GFFileOpenMode.GF_ISOM_WRITE_EDIT, "");
			////
			//import.dest.finalStream = new FileStream("Aloha.mp4", FileMode.Create);//Console.OpenStandardOutput();
			//var parser = new MainParser(import);
			//import.flags = 0;
			//parser.ParseH264(_buffer);
			//parser.FinalFlush();
			//import.dest.gf_isom_close();

		}

		static void Main(string[] args)
		{
			Console.WriteLine("Hello World!");





			try
			{
				var main = new Program();
				if (args.Length == 1)
				{
					if (args[0] == "c")
						main.TestCapture();
					else if (args[0] == "r")
						main.TestRecording(args[0]);
					//else if (args[0] == "w")
					//	main.TestMalVideo("");
					//else if (args[0] == "m")
					//	main.TestMalCamera("");

				}
				main.TestParser();
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex);
			}
		}

		//public void NewMethod()
		//{
		//	_MMalCamera camera = new _MMalCamera();
		//	camera.CreateCameraComponent();
		//	camera.CreatePreview();
		//	camera.CreateEncoderComponent();
		//	camera.FinishInit();
		//	s = new FileStream("Picture1.jpg", FileMode.CreateNew);
		//	camera.Capture(CameraCallback);
		//}

	}
}
