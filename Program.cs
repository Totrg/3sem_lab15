using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Threading;

namespace _3sem_lab15
{

    public interface IObserver
    {
        void Update(string path);
    }

    public class FileObserver
    {
        private List<IObserver> observers = new List<IObserver>();
        private string directoryPath;

        public FileObserver(string path)
        {
            directoryPath = path;
        }

        public void Subscribe(IObserver observer)
        {
            observers.Add(observer);
        }

        public void Unsubscribe(IObserver observer)
        {
            observers.Remove(observer);
        }

        public void CheckDirectory()
        {
            foreach (var observer in observers)
            {
                observer.Update(directoryPath);
            }
        }
    }

    public class ConcreteObserver : IObserver
    {
        private DateTime lastFileChangeTime = DateTime.MinValue;

        public void Update(string filePath)
        {
            DateTime currentFileChangeTime = File.GetLastWriteTime(filePath);

            if (currentFileChangeTime != lastFileChangeTime)
            {
                Console.WriteLine($"File {filePath} has changed!");
                lastFileChangeTime = currentFileChangeTime;
            }
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
            // Пример использования Observer
            FileObserver fileObserver = new FileObserver("C:\\Users\\belog\\Downloads\\testfile.txt");
            ConcreteObserver concreteObserver = new ConcreteObserver();
            fileObserver.Subscribe(concreteObserver);
            Timer timer = new Timer(_ => fileObserver.CheckDirectory(), null, 0, 500);
            
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
