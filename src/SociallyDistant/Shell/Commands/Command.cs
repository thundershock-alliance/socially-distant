﻿using System;
using SociallyDistant.Core;
using Thundershock.Core;
using Thundershock.Core.Debugging;
using Thundershock.Gui.Elements.Console;
using Thundershock.IO;

namespace SociallyDistant.Shell.Commands
{
    public abstract class Command
    {
        private string _home;
        private IProgramContext _userContext;
        private bool _completed;
        private bool _running = false;
        private IConsole _console;
        private FileSystem _fs;
        private string[] _args;
        private string _workingDirectory;
        private bool _disposeConsole;
        
        public abstract string Name { get; }
        public virtual string Description => string.Empty;
        protected IProgramContext Context => _userContext;
        protected IConsole Console => _console;
        protected string[] Arguments => _args;
        protected string WorkingDirectory => _workingDirectory;
        protected FileSystem FileSystem => _fs;
        
        public bool IsCompleted => _completed;

        protected string ResolvePath(string path)
        {
            var resolved = path;
            if (resolved.StartsWith(PathUtils.Home))
                return PathUtils.Resolve(PathUtils.Combine(_home, resolved.Substring(PathUtils.Home.Length)));
            if (!resolved.StartsWith(PathUtils.Separator))
                return PathUtils.Resolve(_workingDirectory, path);
            return PathUtils.Resolve(resolved);
        }
        
        public void Run(string[] args, string work, IConsole console, IProgramContext ctx, bool disposeConsole = true)
        {
            if (_running)
                throw new InvalidOperationException("Command has already been run.");

            _disposeConsole = disposeConsole;
            _userContext = ctx ?? throw new ArgumentNullException(nameof(ctx));            
            _args = args ?? throw new ArgumentNullException(nameof(args));
            _workingDirectory = work ?? throw new ArgumentNullException(nameof(work));
            _fs = ctx.Vfs;
            _console = console ?? throw new ArgumentNullException(nameof(console));
            _home = _userContext.HomeDirectory;
            
            try
            {
                Main(args);
            }
            catch (Exception ex)
            {
                console.WriteLine("{0}: error: {1}", Name, ex.Message);
                Logger.LogException(ex, LogLevel.Warning);
                Complete();
            }
        }

        protected void Complete()
        {
            if (!_completed)
            {
                _completed = true;
                
                // Reset the console formatting state
                _console.WriteLine("&0");

                // dispose of the console if that's necessary.
                if (_console is IDisposable disposable && _disposeConsole)
                {
                    disposable.Dispose();
                }
            }
        }
        
        protected abstract void Main(string[] args);

        protected virtual void OnUpdate(float deltaTime)
        {
            Complete();
        }
        
        public void Update(float deltaTime)
        {
            if (!IsCompleted)
            {
                try
                {
                    OnUpdate(deltaTime);
                }
                catch (Exception ex)
                {
                    if (_console != null)
                    {
                        _console.WriteLine("{0}: error: {1}", Name, ex.Message);
                    }
                    else
                    {
                        Logger.Log(
                            "Could not log command error to the user's screen because the game killed the console instance before we got here.",
                            LogLevel.Error);
                    }

                    Logger.LogException(ex, LogLevel.Warning);
                    Complete();
                }
            }
        }
    }
}