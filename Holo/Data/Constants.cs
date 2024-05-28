using System;
using System.Collections.Generic;
using System.Text;

namespace Holo.Data
{
    internal static class Constants
    {
        public const string FreeformDocumentTemplate = @"
// Welcome to the Ameko Freeform Scripting Playground!
// Here, you can write a ""freeform"" script to automate
// tasks, workshop a script, or just have fun!
using System;
using System.Collections.Generic;
using System.Linq;
using Ameko;
using Holo;
using Holo.Api;
using AssCS;

// Some commonly-used variables to get you started:
Event? selectedEvent = Fern.SelectedEvent;
List<Event>? selectedEventCollection = Fern.SelectedEventCollection;

// Fern provides easy access to common data from the current file.
// For more control, try accessing directly via:
// FileWrapper file = HoloContext.Instance.Workspace.WorkingFile;

selectedEvent.Text = ""Hello from Freeform Scripting!"";
";
    }
}
