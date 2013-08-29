XDebugClient
============

This is a fork of the [original][] XDebugClient that did not get, at the time
of writing, any updates for many years. The goal of this project is to update
the code to work with latest DBGp specifications (and debug engine implementation)
and also add usability features.

[original]: http://code.google.com/p/xdebugclient/

Motivation
----------

In 2007 I released the first version of [DBGP Plugin][dbgpplugin] for [Notepad++][npp].
The project is still (barely) maintained, but it always bugged me that NPPs window
dock manager was so limited. With a debugger, you need a lot of docked windows. When
XDC was first released, I had a look at it, but at that time it was still lacking
a lot of features and I was focusing more on my client. Years later I needed to
setup a PHP debug client for a colleague and said that I'll give XDC another look.
I realized that the implementation was still lacking quite some features and as I
also gained some experience developing and maintaining the Flash Debugger plugin of
[FlashDevelop][fd], I got the code and decided to add some of the basic features
that I'd expect a client to have.
I believe that the client still has huge potential as it is small and very simple
to set up, and I'm posting the changes online so somebody might benefit from them.

[dbgpplugin]: http://sourceforge.net/projects/npp-plugins/files/DBGP%20Plugin/
[npp]: http://notepad-plus-plus.org/
[fd]: http://www.flashdevelop.org/

New features/Improvements
=========================

* Global and Local context

  Look at local and global variables while stepping through code.

* Move up and down the stack

  Whenever execution is paused, inspection of different stack levels is possible
  by doubble clicking on call stack entries.

* Sotring settings

  Docking positions and options are saved to xml files.

TODO
----

* Proxy Register
* Improve how Properties tree updates (Replace Model on new Properties)
* Log/Output window has to support some colors
* On exception show some info about it, in log/output window
* Sort properties
* View property value form close button does not work
* Settings change should take effect right away (send new features to server if connected)
* Raw/Dbg window to log client-server communication and enter commands directly into stream
* Catch disconnects. Clean stack, context window - use BeginRecieve and EventWaitHandle (WaitOne, Set) to wait fpr resp
* Multiple sessions, session window, refactor client class (move to separate thread or make it async)
* Improve properties details (there was some strange casting of "false" to "0")
* "this" has sometimes duplicated data
* Source file reload
* stdout window?
* Breakpints window
* Ctrl-F search, F3 next
* Client refactor: have link to breakpoint manager, move "SendContinuationCommand" to client
* Start listening on startup
* Taskbar