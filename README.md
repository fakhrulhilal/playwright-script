# Playwright Script

A collection of my browser automation script using playwright. 
This is more like a playground and learning for me. It's not intended for anyone.

## Building and Running

You need to install [.NET8](https://get.dot.net/8). It's just like another .NET project, you can just use `dotnet run` 
to run the project. You can follow [official guide](https://playwright.dev/dotnet/docs/browsers) to install the browsers. 
But I prefer to use existing browser and re-use existing session/cookies. Remember, it's playground scripts, not intended 
for doing serious office job 😆. 
To create session/cookie for MS Edge, run this command after build (`dotnet build`): 
```pwsh
New-Item -ItemType Directory -Path .auth
.\bin\Debug\net8.0\playwright.ps1 codegen 
    --target csharp 
    -b chromium 
    --channel msedge 
    --save-storage .\.auth\state.json
```

## Scripts

- [`Helper`](Helper.cs)
  
  It's just a wrapper for doing playwright thing, nothing special. 
  It will find proper browser installation location. 
  This will also load the state when found.
- [`Tokopedia`](Tokopedia.cs)
  - `CleanupWishlist`: cleaning up long wishlist
                                                                                                                                                     |