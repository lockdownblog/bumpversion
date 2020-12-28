namespace BumpVersion
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using McMaster.Extensions.CommandLineUtils;

    public class BumpVersion
    {
        [Argument(0)]
        [Required]
        public string Part { get; }

        public static int Main(string[] args)
        {
            return CommandLineApplication.Execute<BumpVersion>(args);
        }

        private void OnExecute()
        {
            Console.WriteLine($"Hello! you will bump \"{this.Part}\".");
        }
    }
}
