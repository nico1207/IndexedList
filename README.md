# IndexedList

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
var indexedList = new IndexedList<Book>(b => b.Pages);
```

In the constructor's parameters you need to specify all properties that should be indexed.

In order to retrieve the books by the created index you need to use `WhereIndexed`:

```csharp
foreach(Book book in indexedList.WhereIndexed(b => b.Pages, 50)) 
{
    Console.WriteLine(book.Name);    
}
```