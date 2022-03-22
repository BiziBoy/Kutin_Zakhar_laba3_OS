using System;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace Kutin_Zakhar_laba3_OS
{
  class Producer
  {
    private ChannelWriter<int> Writer;

    public Producer(ChannelWriter<int> _writer)
    {
      Writer = _writer;
      Task.WaitAll(Run());
    }

    private async Task Run()
    {
      var r = new Random();
      //ожидает, когда освободиться место для записи элемента.
      while (await Writer.WaitToWriteAsync())
      {
        if (Program.flag && Program.count <= 100)
        {
          var item = r.Next(1, 101);
          await Writer.WriteAsync(item);
          Program.count += 1;
          Console.WriteLine($"Записанные данные: {item}");
        }
        else Console.WriteLine("!!!Конвейер заполнен!!!");
      }
    }
  }


class Consumer
  {
    private ChannelReader<int> Reader;

    public Consumer(ChannelReader<int> _reader)
    {
      Reader = _reader;
      Task.WaitAll(Run());
    }

    private async Task Run()
    {
      // ожидает, когда освободиться место для чтения элемента.
      while (await Reader.WaitToReadAsync())
      {
        if (Reader.Count != 0)
        {
          var item = await Reader.ReadAsync();
          Program.count -= 1;
          Console.WriteLine($"Полученные данные: {item}");
        }
        if (Reader.Count >= 100)
        {
          Program.flag = false;
        }
        else if (Reader.Count <= 80)
        {
          Program.flag = true;
        }
      }
    }
  }

  class Program
  {
    static public bool flag = true;
    static public int count = 0;

    static void Main(string[] args)
    {
      //создаю общий канал данных
      Channel<int> channel = Channel.CreateBounded<int>(200);
      //создаются производители и потребители
      Task[] streams = new Task[5]; 
      for (int i = 0; i < 5; i++)
      {
        if (i < 3)
        {
          streams[i] = Task.Run(() => { new Producer(channel.Writer); });
        }
        else
        {
          streams[i] = Task.Run(() => { new Consumer(channel.Reader); });
        }
      }
      //Ожидает завершения выполнения всех указанных объектов Task 
      Task.WaitAll(streams);
    }
  }
}
