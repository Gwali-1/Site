---
{
  "title": "Why I Like Building Things From Scratch",
  "tags": ["sample", "markdown"],
  "date": "2026-03-20",
  "slug": "sample-blog-post"
}
---

# 

A lot of developers eventually reach a point where they want to understand what is happening underneath the abstractions. For me, that curiosity started when I wrote a small HTTP server using `TcpListener`.

> Building things from scratch forces you to understand the fundamentals instead of depending entirely on frameworks.

## Things I Learned

- How HTTP requests are structured
- Why asynchronous programming matters
- How sockets work underneath web frameworks
- The difference between blocking and non-blocking operations

Working directly with streams and sockets made concepts feel more real. Something as simple as reading bytes from a stream suddenly became interesting.

## Small Example

Inline code can be useful for mentioning methods like `Task.Run`, `await`, or `HttpListener`.

Here is a simple example:

```csharp
var listener = new TcpListener(IPAddress.Any, 8080);

listener.Start();

while (true)
{
    var client = await listener.AcceptTcpClientAsync();

    Console.WriteLine("Client connected");
}
```

## Useful Resources

- [Microsoft .NET Documentation](https://learn.microsoft.com/dotnet/)
- [MDN HTTP Overview](https://developer.mozilla.org/en-US/docs/Web/HTTP/Overview)
- [Beej's Guide to Network Programming](https://beej.us/guide/bgnet/)

## Final Thoughts

I still enjoy using frameworks, but understanding lower-level concepts makes debugging and designing systems much easier.

> The abstractions become more useful once you understand what they are abstracting away.