using Microsoft.Deployment.Compression;
using Microsoft.Deployment.Compression.Cab;
using System;

namespace Pack
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                const string CabFile = @"E:\Gits\sample code\TestQML.cab";
                const string CabFolder = @"E:\Gits\sample code\TestQML";
                var cabInfo = new CabInfo(CabFile);
                cabInfo.Pack(CabFolder,true, Microsoft.Deployment.Compression.CompressionLevel.Max, Hanler);
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        private static void Hanler(object sender, ArchiveProgressEventArgs e)
        {
            Console.WriteLine(e.CurrentFileName);
        }
    }
}
