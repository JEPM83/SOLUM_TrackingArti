using System;
using System.Collections.Generic;
using System.Text;
using Zebra.Sdk.Printer.Discovery;


namespace SolumInfraestructure.Service
{
    public class ZebraPrintHelper
    {
    }

    public class BluetoothDiscoveryHandler : DiscoveryHandler
    {

        private bool discoveryComplete = false;
        List<DiscoveredPrinter> printers = new List<DiscoveredPrinter>();

        public void DiscoveryError(string message)
        {
            Console.WriteLine($"An error occurred during discovery: {message}.");
            discoveryComplete = true;
        }

        public void DiscoveryFinished()
        {
            foreach (DiscoveredPrinter printer in printers)
            {
                Console.WriteLine(printer);
            }
            Console.WriteLine($"Discovered {printers.Count} Bluetooth printers.");
            discoveryComplete = true;
        }

        public void FoundPrinter(DiscoveredPrinter printer)
        {
            printers.Add(printer);
        }

        public bool DiscoveryComplete
        {
            get => discoveryComplete;
        }
    }

    public class NetworkDiscoveryHandler : DiscoveryHandler
    {

        private bool discoveryComplete = false;
        List<DiscoveredPrinter> printers = new List<DiscoveredPrinter>();

        public void DiscoveryError(string message)
        {
            Console.WriteLine($"An error occurred during discovery: {message}.");
            discoveryComplete = true;
        }

        public void DiscoveryFinished()
        {
            foreach (DiscoveredPrinter printer in printers)
            {
                Console.WriteLine(printer);
            }
            Console.WriteLine($"Discovered {printers.Count} Link-OS™ printers.");
            discoveryComplete = true;
        }

        public void FoundPrinter(DiscoveredPrinter printer)
        {
            printers.Add(printer);
        }

        public bool DiscoveryComplete
        {
            get => discoveryComplete;
        }
    }

}
