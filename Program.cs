using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Threading;

namespace _3sem_lab15
{

    public class FileObserver
    {
        private static DateTime LastFileChangeTime = DateTime.MinValue;
        public event EventHandler<string> FileChanged;

        private string directoryPath;

        public FileObserver(string path)
        {
            directoryPath = path;
        }

        public void Subscribe(EventHandler<string> handler)
        {
            FileChanged += handler;
        }

        public void Unsubscribe(EventHandler<string> handler)
        {
            FileChanged -= handler;
        }

        public void CheckDirectory()
        {
            string[] files = Directory.GetFiles(directoryPath);

            foreach (var file in files)
            {
                DateTime currentFileChangeTime = File.GetLastWriteTime(file);

                if (currentFileChangeTime != LastFileChangeTime)
                {
                    OnFileChanged(file);
                    LastFileChangeTime = currentFileChangeTime;
                }
            }
        }

        protected virtual void OnFileChanged(string filePath)
        {
            FileChanged?.Invoke(this, filePath);
        }
    }

    public class ConcreteObserver
    {
        public void HandleFileChange(object sender, string filePath)
        {
            Console.WriteLine($"File {filePath} has changed");
        }
    }


    public class MyLogger
    {
        private StreamWriter streamWriter;

        public MyLogger(string filePath)
        {
            streamWriter = new StreamWriter(filePath);
        }

        public void Log(string message)
        {
            streamWriter.WriteLine($"{DateTime.Now:yyyy-MM-dd HH:mm:ss} - {message}");
        }

        public void LogToJson(string message)
        {
            streamWriter.WriteLine($"{{ \"timestamp\": \"{DateTime.Now:yyyy-MM-dd HH:mm:ss}\", \"message\": \"{message}\" }}");
        }

        public void Close()
        {
            streamWriter.Close();
        }
    }

    public class SingleRandomizer
    {
        private static SingleRandomizer instance;
        private static readonly object lockObject = new object();
        private Random random;

        private SingleRandomizer()
        {
            random = new Random();
        }

        public static SingleRandomizer Instance
        {
            get
            {
                lock (lockObject)
                {
                    if (instance == null)
                    {
                        instance = new SingleRandomizer();
                    }
                    return instance;
                }
            }
        }

        public int GetNextRandomNumber()
        {
            lock (lockObject)
            {
                return random.Next();
            }
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            FileObserver fileObserver = new FileObserver("C:\\Users\\belog\\Downloads\\testFolder");
            ConcreteObserver concreteObserver = new ConcreteObserver();
            fileObserver.Subscribe(concreteObserver.HandleFileChange);
            Timer timer = new Timer(_ => fileObserver.CheckDirectory(), null, 0, 5000);

            while (Console.ReadKey().Key != ConsoleKey.Q) { }

            timer.Dispose();
            Console.ReadLine();
            
            //Пример использования Repository
            MyLogger logger = new MyLogger("C:\\Users\\belog\\Downloads\\testfile.txt");
            logger.Log("Log message 1");
            logger.LogToJson("Log message 2");
            logger.Close(); // Закрытие потока при завершении работы
            
            //Пример использования Singleton
            SingleRandomizer randomizer = SingleRandomizer.Instance;
            Thread thread1 = new Thread(() => Console.WriteLine($"Thread 1: {randomizer.GetNextRandomNumber()}"));
            Thread thread2 = new Thread(() => Console.WriteLine($"Thread 2: {randomizer.GetNextRandomNumber()}"));
            thread1.Start();
            thread2.Start();
            thread1.Join();
            thread2.Join();
            Console.ReadLine();
        }
    }
}
