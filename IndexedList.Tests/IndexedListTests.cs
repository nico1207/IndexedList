using System.Collections.Generic;
using Xunit;
using IndexedList;

namespace IndexedList.Tests;

public class IndexedListTests
{
    [Fact]
    public void AddAndQuery_ByIndexedProperty()
    {
        var list = new IndexedList.IndexedList<Book>(b => b.Pages, b => b.Name);
        var book1 = new Book { Name = "A", Pages = 100 };
        var book2 = new Book { Name = "B", Pages = 200 };
        list.Add(book1);
        list.Add(book2);

        var books = list.WhereIndexed(b => b.Pages, 100);
        Assert.Contains(book1, books);
        Assert.DoesNotContain(book2, books);
    }

    [Fact]
    public void RemoveItem_RemovesFromIndices()
    {
        var list = new IndexedList.IndexedList<Book>(b => b.Pages);
        var book1 = new Book { Name = "A", Pages = 100 };
        list.Add(book1);
        list.Remove(book1);

        Assert.Throws<KeyNotFoundException>(() => list.WhereIndexed(b => b.Pages, 100));
    }

    [Fact]
    public void IndexerUpdate_RefreshesIndices()
    {
        var list = new IndexedList.IndexedList<Book>(b => b.Pages);
        var book1 = new Book { Name = "A", Pages = 100 };
        var book2 = new Book { Name = "B", Pages = 200 };
        list.Add(book1);

        list[0] = book2;

        Assert.Throws<KeyNotFoundException>(() => list.WhereIndexed(b => b.Pages, 100));
        var books = list.WhereIndexed(b => b.Pages, 200);
        Assert.Contains(book2, books);
    }
}
