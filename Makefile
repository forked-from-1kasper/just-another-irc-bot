MUSTHAVE=-r IRCBot.dll -r IRCBot.Public.Prefix.dll -r IRCBot.Public.Constants.dll
MSCORLIB="c:\Windows\Microsoft.NET\Framework\v4.0.30319\mscorlib.dll"

FSHARP=fsc

all: core public modules
	echo Done!

core:
	echo Building IRCBot.dll
	$(FSHARP) -a IRCBot.fs
	echo [×] core done

public: prefix constants
	echo [×] public done

prefix:
	echo Building IRCBot.Public.Prefix
	$(FSHARP) -a IRCBot.Public.Prefix.fs

constants:
	echo Building IRCBot.Public.Constants
	$(FSHARP) -a IRCBot.Public.Constants.fs

modules: vote jokes title sample punto
	echo [×] modules done

vote:
	echo Building IRCBot.Modules.Vote
	$(FSHARP) -a $(MUSTHAVE) IRCBot.Modules.Vote.fs

jokes:
	echo Building IRCBot.Modules.Jokes
	$(FSHARP) -a $(MUSTHAVE) IRCBot.Modules.Jokes.fs

title:
	echo Building IRCBot.Modules.Title
	$(FSHARP) -a $(MUSTHAVE) -r FSharp.Data.dll IRCBot.Modules.Title.fs

markov:
	echo Building Markov.dll
	$(FSHARP) -a $(MUSTHAVE) Markov.fs

sample: markov
	echo Building IRCBot.Modules.Sample
	$(FSHARP) -a $(MUSTHAVE) -r Markov.dll IRCBot.Modules.Sample.fs

punto:
	echo Building IRCBot.Modules.Punto
	$(FSHARP) -a $(MUSTHAVE) IRCBot.Modules.Punto.fs

clean:
	rm IRCBot.dll
	rm IRCBot.*.dll
	echo cleaning done
