using System;
using Xunit;
using Docker.DotNet;
using Docker.DotNet.Models;
using System.Runtime.InteropServices;
using System.IO;
using System.Linq;
using ICSharpCode.SharpZipLib.Tar;
using System.Collections.Generic;

namespace BumpVersion.Tests
{
    public class EndToEndTest
    {
        DockerClient dockerClient;
        public EndToEndTest()
        {
            Uri uri;
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                uri = new Uri("npipe://./pipe/docker_engine");
            }
            else
            {
                uri = new Uri("unix:///var/run/docker.sock");
            }
            dockerClient = new DockerClientConfiguration(uri).CreateClient();
        }

        [Fact]
        public async void Test1()
        {
            var x = Directory.GetCurrentDirectory();
            var path = String.Join('/', x.Split('\\').SkipLast(4));
            Assert.Equal(@"C:/Users/anton/GitHub/bumpversion", path);


                var parameters = new ImageBuildParameters()
                {
                    Dockerfile = "BumpVersion.Tests/Docker/Dockerfile.end2end",
                    Tags = new List<string> { "test1" },
                };
            using var tarball = CreateTarballForDockerfileDirectory(path);
            using var responseStream = await dockerClient.Images.BuildImageFromDockerfileAsync(tarball, parameters);

            var container = await dockerClient.Containers.CreateContainerAsync(new CreateContainerParameters { Image = "test1", AttachStderr=true, StdinOnce=true, OpenStdin=true, AttachStdout = true, AttachStdin=true,  Tty =true });
            await dockerClient.Containers.StartContainerAsync(container.ID, new ContainerStartParameters { });

            var result = await dockerClient.Exec.ExecCreateContainerAsync(container.ID, new ContainerExecCreateParameters { Cmd = new List<string> { "cat /test/.bumpversion.cfg" } });
           var response= await dockerClient.Containers.InspectContainerAsync(container.ID);
        }
        private static Stream CreateTarballForDockerfileDirectory(string directory)
        {
            var tarball = new MemoryStream();
            var files = Directory.GetFiles(directory, "*.*", SearchOption.AllDirectories);

            using var archive = new TarOutputStream(tarball)
            {
                //Prevent the TarOutputStream from closing the underlying memory stream when done
                IsStreamOwner = false
            };

            foreach (var file in files)
            {
                //Replacing slashes as KyleGobel suggested and removing leading /
                string tarName = file.Substring(directory.Length).Replace('\\', '/').TrimStart('/');

                var parts = tarName.Split('/');

                if (parts.Contains(".vs") || parts.Contains(".git"))
                    continue;

                //Let's create the entry header
                var entry = TarEntry.CreateTarEntry(tarName);
                using var fileStream = File.OpenRead(file);
                entry.Size = fileStream.Length;
                archive.PutNextEntry(entry);

                //Now write the bytes of data
                byte[] localBuffer = new byte[32 * 1024];
                while (true)
                {
                    int numRead = fileStream.Read(localBuffer, 0, localBuffer.Length);
                    if (numRead <= 0)
                        break;

                    archive.Write(localBuffer, 0, numRead);
                }

                //Nothing more to do with this entry
                archive.CloseEntry();
            }
            archive.Close();

            //Reset the stream and return it, so it can be used by the caller
            tarball.Position = 0;
            return tarball;
        }
    }
}
