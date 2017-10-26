MUSTHAVE=-r IRCBot.dll -r IRCBot.Public.Prefix.dll -r IRCBot.Public.Constants.dll
MSCORLIB="c:\Windows\Microsoft.NET\Framework\v4.0.30319\mscorlib.dll"

all: core public modules

core:
	fsc -a IRCBot.fs

public: prefix constants

prefix:
	fsc -a IRCBot.Public.Prefix.fs

constants:
	fsc -a IRCBot.Public.Constants.fs

modules: vote jokes title sample punto

vote:
	fsc -a $(MUSTHAVE) IRCBot.Modules.Vote.fs

jokes:
	fsc -a $(MUSTHAVE) IRCBot.Modules.Jokes.fs

title:
	fsc -a $(MUSTHAVE) -r FSharp.Data.dll IRCBot.Modules.Title.fs

markov:
	fsc -a $(MUSTHAVE) Markov.fs

sample: markov
	fsc -a $(MUSTHAVE) -r Markov.dll IRCBot.Modules.Sample.fs

punto:
	fsc -a $(MUSTHAVE) IRCBot.Modules.Punto.fs

clean:
	rm IRCBot.dll
	rm IRCBot.*.dll
