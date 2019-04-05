using SharpDX.Diagnostics;
using System.IO;

namespace MazeEditor
{
    public class ObjectTrackerToFile
    {
        private string _fileName;
        private string _logPath;
        private int _fileTracker = 0;

        public ObjectTrackerToFile(string logPath)
        {
            _logPath = logPath;
        }

        public void TrackLiveObjects()
        {
            string filePath = _logPath + $"\\frame{_fileTracker}.txt";

            File.AppendAllText(filePath, ObjectTracker.ReportActiveObjects());

            _fileTracker++;
        }

    }
}
