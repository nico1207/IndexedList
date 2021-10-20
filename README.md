# IndexedList
<h3>

  [![NuGet](https://img.shields.io/nuget/v/IndexedList.svg)](https://www.nuget.org/packages/IndexedList/)
  [![License](https://img.shields.io/badge/license-MIT-blue.svg)](LICENSE.md)

</h3>

This library provides a specialized collection for faster list lookups by creating dictionaries for specified properties of the list's elements. It's inspired by [this article](https://www.c-sharpcorner.com/article/indexing-in-memory-collections-for-blazing-fast-access/) by Joel Champagne.

## Example

Filtering a list by some property is a common usecase. This example will use an entity that looks like this:

```csharp
public class Book
{
    public string Name {get; set;}
    public int Pages {get; set;}
}
```

If you want to retrieve all books from a list that have a specific amount of pages you could it solve it using LINQ's `Where` operation:


```csharp
foreach(Book book in books.Where(b => b.Pages == 50)) 
{
    Console.WriteLine(book.Name);    
}
```

This operation will iterate over every element in the list and compare the property's value.

To speed this up one could create a `Dictionary` that maps the amount of pages of a book to a list of all books with that amount.

The `IndexedList` provided by this library will automatically do that for you. We will start by creating a new `IndexedList`:

```csharp
var indexedList = new IndexedList<Book>(b => b.Pages, b => b.Name);
```

In the constructor's parameters you need to specify all properties that should be indexed.

In order to retrieve the books using the created index you need to use `WhereIndexed`:

```csharp
foreach(Book book in indexedList.WhereIndexed(b => b.Pages, 50)) 
{
    Console.WriteLine(book.Name);    
}
```

## Benchmark

```
|               Method |      Mean |     Error |    StdDev |    Median |  Gen 0 | Allocated |
|--------------------- |----------:|----------:|----------:|----------:|-------:|----------:|
|        BenchmarkList | 75.107 us | 1.4833 us | 3.6940 us | 73.917 us |      - |      72 B |
| BenchmarkIndexedList |  1.942 us | 0.0325 us | 0.0543 us |  1.928 us | 0.2174 |     688 B |


```

As you can see the `IndexedList` provides a roughly ~40x speedup as compared to a regular list and LINQ's `Where` operation. This comes at the cost of memory allocations which are the result of boxing operations and the underlying dictionaries.

The benchmarks were conducted using the following code:

```csharp
[MemoryDiagnoser()]
public class Benchmarks
{
    private IndexedList<Book> indexlist;
    private List<Book> list;

    [GlobalSetup]
    public void Setup()
    {
        indexlist = new IndexedList<Book>(p => p.Pages);
        list = new List<Book>();

        Random random = new Random();
        for (int i = 0; i < 10000; i++)
        {
            var book = new Book()
            {
                Pages = random.Next(100),
                Name = random.Next().ToString()
            };
            
            indexlist.Add(book);
            list.Add(book);
        }
    }

    [Benchmark]
    public int BenchmarkList()
    {
        int totalPages = 0;
        
        foreach (var book in list.Where(p => p.Pages == 50))
        {
            totalPages += book.Pages;
        }

        return totalPages;
    }
    
    [Benchmark]
    public int BenchmarkIndexedList()
    {
        int totalPages = 0;
        
        foreach (var person in indexlist.WhereIndexed(p => p.Pages, 50))
        {
            totalPages += person.Pages;
        }

        return totalPages;
    }
}
```

## Installation

You can use this library in your project by adding the following [NuGet package](https://www.nuget.org/packages/IndexedList/):

```
Install-Package IndexedList
```