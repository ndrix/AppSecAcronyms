# App Sec Acronyms

The infosec world is filled with acronyms, xxe, ssrf, ssti and rce's.  This little site would want to make
it a bit easier to search these terms, but it may suffer from a few security bugs.

This is a small intentionally vulnerable application, which is the code used for the Microsoft Security Community
Webinar on SPA treatments (https://aka.ms/SecurityWebinars).  There are three bugs that we know off:

 * Dom XSS
 * Stored XSS
 * SSRF
 
This is written in .NET Core 3.1, and should work with any Azure Storage account, and Ms SQL server and database.  It's not
the prettiest code, but it does the job to give our automated scanners a run for it.

Note that this is not official Microsoft code that is shipped, this is purely for educational purposes.  When you run this 
in your environment, make sure that you restrict network acecss to it accordingly.

For any comments and questions, feel free to contact me at [mihendri@microsoft.com](mailto:mihendri@microsoft.com)
