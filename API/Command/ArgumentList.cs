﻿using System;
using System.Collections.Generic;
using System.Linq;

#if Full_API
using Terraria;
#endif
namespace OTA.Command
{
    /// <summary>
    /// A OTA exception for throwing a command error to the sender with the help usage + message
    /// </summary>
    public class CommandError : ApplicationException
    {
        /// <summary>
        /// Sends a message with the help text
        /// </summary>
        /// <param name="message">Message.</param>
        public CommandError(string message) : base(message)
        {
        }

        /// <summary>
        /// Sends a formatted message
        /// </summary>
        /// <param name="fmt">Fmt.</param>
        /// <param name="args">Arguments.</param>
        public CommandError(string fmt, params object[] args) : base(String.Format(fmt, args))
        {
        }
    }

    /// <summary>
    /// The list of arguments received from a command sender
    /// </summary>
    public class ArgumentList : List<string>
    {
        /// <summary>
        /// The plugin instance if the executed command was a plugin command
        /// </summary>
        /// <value>The plugin.</value>
        public object Plugin { get; set; }

        public ArgumentList()
        {
        }

        static readonly Dictionary<System.Type, string> typeNames = new Dictionary<System.Type, string>()
        {
            { typeof(string),   "a string" },
            { typeof(int),      "an integer number" },
            { typeof(double),   "a number" },
            { typeof(bool),     "a boolean value" },
			#if Full_API
            { typeof(Player),   "an online player's name" },
			#endif
            { typeof(TimeSpan), "a duration" },
        };

        static readonly Dictionary<string, bool> booleanValues = new Dictionary<string, bool>()
        {
            { "true", true }, { "yes", true }, { "+", true }, { "1", true },
            { "enable", true }, { "enabled", true }, { "on", true },
			
            { "false", false }, { "no", false }, { "-", false }, { "0", false },
            { "disable", false }, { "disabled", false }, { "off", false },
        };

        /// <summary>
        /// Gets a string at a expected position
        /// </summary>
        /// <returns>The string.</returns>
        public string GetString(int at)
        {
            if (at >= Count) throw new CommandError("Too few arguments given.");

            return this[at];
        }

        /// <summary>
        /// Attempts to get a string at a specified location
        /// </summary>
        public bool TryGetString(int at, out string val)
        {
            val = null;

            if (at >= Count) return false;

            val = this[at];

            return true;
        }

        /// <summary>
        /// Parses an integer at a specified location. It will warn the sender if it's invalid
        /// </summary>
        /// <returns>The int.</returns>
        public int GetInt(int at)
        {
            if (at >= Count) throw new CommandError("Too few arguments given.");

            int val;
            if (Int32.TryParse(this[at], out val))
            {
                return val;
            }

            throw new CommandError("An integer number was expected for argument {0}.", at + 1);
        }

        /// <summary>
        /// Attempts to parse an integer at an expected position
        /// </summary>
        public bool TryGetInt(int at, out int val)
        {
            val = -1;

            if (at >= Count) return false;

            return Int32.TryParse(this[at], out val);
        }

        /// <summary>
        /// Parses a byte at an expected position. If it is invalid it will warn the sender.
        /// </summary>
        /// <returns>The byte.</returns>
        public byte GetByte(int at)
        {
            if (at >= Count) throw new CommandError("Too few arguments given.");

            byte val;
            if (Byte.TryParse(this[at], out val))
            {
                return val;
            }

            throw new CommandError("An byte value [0-255] was expected for argument {0}.", at + 1);
        }

        /// <summary>
        /// Attempts to parse a byte value at an expected position
        /// </summary>
        public bool TryGetByte(int at, out byte val)
        {
            val = 0;

            if (at >= Count) return false;

            return Byte.TryParse(this[at], out val);
        }

        /// <summary>
        /// Parses a double value at an expected position. If it fails the sender will be notified.
        /// </summary>
        /// <returns>The double.</returns>
        public double GetDouble(int at)
        {
            if (at >= Count) throw new CommandError("Too few arguments given.");

            double val;
            if (Double.TryParse(this[at], out val))
            {
                return val;
            }

            throw new CommandError("A number was expected for argument {0}.", at + 1);
        }

        /// <summary>
        /// Attempts to get a double value at an expected position
        /// </summary>
        public bool TryGetDouble(int at, out double val)
        {
            val = -1;

            if (at >= Count) return false;

            return Double.TryParse(this[at], out val);
        }

        /// <summary>
        /// Parses a TimeSpan at an expected position. If it fails the sender will be notified.
        /// </summary>
        /// <returns>The duration.</returns>
        public TimeSpan GetDuration(int at)
        {
            if (at >= Count) throw new CommandError("Too few arguments given.");

            TimeSpan val;
            if (TryGetDuration(at, out val))
                return val;

            throw new CommandError("A duration was expected for argument {0}.", at + 1);
        }

        /// <summary>
        /// Attempts to parse a TimeSpan at an expected location.
        /// </summary>
        public bool TryGetDuration(int at, out TimeSpan val)
        // TODO: Add support for composite duration literals, ie: 4h30m15s
        {
            val = TimeSpan.FromSeconds(0);

            if (at >= Count) return false;

            double value;
            double scale = 1.0;
            var str = this[at];

            if (str.EndsWith("ms"))
            {
                scale = 1e-3;
                str = str.Substring(0, str.Length - 2);
            }
            else if (str.EndsWith("us"))
            {
                scale = 1e-6;
                str = str.Substring(0, str.Length - 2);
            }
            else if (str.EndsWith("s"))
            {
                scale = 1.0;
                str = str.Substring(0, str.Length - 1);
            }
            else if (str.EndsWith("m"))
            {
                scale = 60.0;
                str = str.Substring(0, str.Length - 1);
            }
            else if (str.EndsWith("h"))
            {
                scale = 3600.0;
                str = str.Substring(0, str.Length - 1);
            }
            else if (str.EndsWith("d"))
            {
                scale = 24 * 3600.0;
                str = str.Substring(0, str.Length - 1);
            }
            else if (str.EndsWith("mo"))
            {
                scale = 30 * 24 * 3600.0;
                str = str.Substring(0, str.Length - 2);
            }
            else if (str.EndsWith("y"))
            {
                scale = 365 * 24 * 3600.0;
                str = str.Substring(0, str.Length - 1);
            }
            else if (str.EndsWith("yr"))
            {
                scale = 365 * 24 * 3600.0;
                str = str.Substring(0, str.Length - 2);
            }

            if (Double.TryParse(str, out value))
            {
                val = TimeSpan.FromSeconds(value * scale);
                return true;
            }

            return false;
        }

        /// <summary>
        /// Parses a boolean value at a specified position.
        /// </summary>
        /// <returns>The bool.</returns>
        public bool GetBool(int at)
        {
            if (at >= Count) throw new CommandError("Too few arguments given.");

            var lower = this[at].ToLower();
            bool val;
            if (booleanValues.TryGetValue(lower, out val))
            {
                return val;
            }

            throw new CommandError("An boolean value was expected for argument {0}.", at + 1);
        }

        /// <summary>
        /// Attempts to parse various values of boolean representations.
        /// </summary>
        public bool TryGetBool(int at, out bool val)
        {
            val = false;

            if (at >= Count) return false;

            var lower = this[at].ToLower();
            return booleanValues.TryGetValue(lower, out val);
        }

        #if Full_API
        /// <summary>
        /// Get's an onine player by name using the argument at a specified position.
        /// </summary>
        /// <returns>The online player.</returns>
        public Player GetOnlinePlayer(int at)
        {
            if (at >= Count) throw new CommandError("Too few arguments given.");

            Player player = Tools.GetPlayerByName(this[at]);

            if (player != null)
                return player;

            var matches = Tools.FindPlayerByPart(this[at]);
            if (matches.Count == 1)
            {
                return matches.ToArray()[0];
            }

            throw new CommandError("A connected player's name was expected for argument {0}.", at + 1);
        }

        /// <summary>
        /// Attempts to get a player by name at a specified position
        /// </summary>
        public bool TryGetOnlinePlayer(int at, out Player val)
        {
            val = null;

            if (at >= Count) return false;

            val = Tools.GetPlayerByName(this[at]);

            if (val != null)
                return true;

            var matches = Tools.FindPlayerByPart(this[at]);
            if (matches.Count == 1)
            {
                val = matches.ToArray()[0];
                return true;
            }

            return false;
        }
        #endif

        /// <summary>
        /// Tries to parse a value at a specified position.
        /// </summary>
        /// <returns>The <see cref="System.Boolean"/>.</returns>
        bool TryParseAt<T>(int at, out T t)
        {
            t = default(T);

            if (typeof(T) == typeof(string))
            {
                string val;
                if (TryGetString(at, out val))
                {
                    t = (T)(object)val;
                    return true;
                }
                return false;

            }
            else if (typeof(T) == typeof(int))
            {
                int val;
                if (TryGetInt(at, out val))
                {
                    t = (T)(object)val;
                    return true;
                }
                return false;
            }
            else if (typeof(T) == typeof(bool))
            {
                bool val;
                if (TryGetBool(at, out val))
                {
                    t = (T)(object)val;
                    return true;
                }
                return false;
            }
#if Full_API
            else if (typeof(T) == typeof(Player))
            {
                Player val;
                if (TryGetOnlinePlayer(at, out val))
                {
                    t = (T)(object)val;
                    return true;
                }
                return false;
            }
#endif
            else if (typeof(T) == typeof(double))
            {
                double val;
                if (TryGetDouble(at, out val))
                {
                    t = (T)(object)val;
                    return true;
                }
                return false;
            }
            else if (typeof(T) == typeof(TimeSpan))
            {
                TimeSpan val;
                if (TryGetDuration(at, out val))
                {
                    t = (T)(object)val;
                    return true;
                }
                return false;
            }
            else if (typeof(T) == typeof(WorldTime))
            {
                var val = WorldTime.Parse(this[at]);
                if (val != null)
                {
                    var gt = val.Value.GameTime;
                    if (gt >= WorldTime.TimeMin && gt <= WorldTime.TimeMax)
                    {
                        t = (T)(object)val.Value;
                        return true;
                    }
                    else throw new CommandError("Invalid time.");
                }
                return false;
            }

            throw new CommandError("Internal command error, type is unsupported by parser: {0}.", typeof(T).ToString());
        }

        /// <summary>
        /// Tries to parse one value at the first position.
        /// </summary>
        public bool TryParseOne<T>(out T t)
        {
            t = default(T);

            if (Count != 1) return false;

            return TryParseAt(0, out t);
        }

        /// <summary>
        /// Tries to parse a value using literals.
        /// </summary>
        public bool TryParseOne<T>(string literal1, out T t, string literal2 = null)
        {
            t = default(T);
            int start = (literal1 != null ? 1 : 0);
            int args = 1 + start + (literal2 != null ? 1 : 0);

            if (Count != args) return false;

            if (literal1 != null && this[0] != literal1)
                return false;

            if (literal2 != null && this[start + 1] != literal2)
                return false;

            return TryParseAt(start, out t);
        }

        /// <summary>
        /// Tried to parse two values sequentially
        /// </summary>
        public bool TryParseTwo<T, U>(out T t, out U u)
        {
            t = default(T);
            u = default(U);

            if (Count != 2) return false;

            return TryParseAt(0, out t) && TryParseAt(1, out u);
        }

        /// <summary>
        /// Tries to parse two values using literals
        /// </summary>
        public bool TryParseTwo<T, U>(string literal1, out T t, string literal2, out U u, string literal3 = null)
        {
            t = default(T);
            u = default(U);
            int arg1 = (literal1 != null ? 1 : 0);
            int arg2 = arg1 + (literal2 != null ? 1 : 0) + 1;
            int args = 2 + arg1 + (literal2 != null ? 1 : 0) + (literal3 != null ? 1 : 0);

            if (Count != args) return false;

            if (literal1 != null && this[0] != literal1)
                return false;

            if (literal2 != null && this[arg1 + 1] != literal2)
                return false;

            if (literal3 != null && this[arg2 + 1] != literal3)
                return false;

            return TryParseAt(arg1, out t) && TryParseAt(arg2, out u);
        }

        /// <summary>
        /// Tries to parse three values
        /// </summary>
        public bool TryParseThree<T, U, V>(out T t, out U u, out V v)
        {
            t = default(T);
            u = default(U);
            v = default(V);

            if (Count != 3) return false;

            return TryParseAt(0, out t) && TryParseAt(1, out u) && TryParseAt(2, out v);
        }

        /// <summary>
        /// Tries to parse three values using literals
        /// </summary>
        public bool TryParseThree<T, U, V>(string literal1, out T t, string literal2, out U u, string literal3, out V v, string literal4 = null)
        {
            t = default(T);
            u = default(U);
            v = default(V);
            int arg1 = (literal1 != null ? 1 : 0);
            int arg2 = arg1 + (literal2 != null ? 1 : 0) + 1;
            int arg3 = arg2 + (literal3 != null ? 1 : 0) + 1;
            int args = arg3 + (literal4 != null ? 1 : 0) + 1;

            if (Count != args) return false;

            if (literal1 != null && this[0] != literal1)
                return false;

            if (literal2 != null && this[arg1 + 1] != literal2)
                return false;

            if (literal3 != null && this[arg2 + 1] != literal3)
                return false;

            if (literal4 != null && this[arg3 + 1] != literal4)
                return false;

            return TryParseAt(arg1, out t) && TryParseAt(arg2, out u) && TryParseAt(arg3, out v);
        }

        /// <summary>
        /// Tries to parse four values
        /// </summary>
        public bool TryParseFour<T, U, V, W>(out T t, out U u, out V v, out W w)
        {
            t = default(T);
            u = default(U);
            v = default(V);
            w = default(W);

            if (Count != 4) return false;

            return TryParseAt(0, out t) && TryParseAt(1, out u) && TryParseAt(2, out v) && TryParseAt(3, out w);
        }

        /// <summary>
        /// Check to ensure no parameters were passed from the sender. If they have provided some a warning will be sent.
        /// </summary>
        public void ParseNone()
        {
            if (Count != 0)
                throw new CommandError("No arguments expected.");
        }

        /// <summary>
        /// Parses the first parameter
        /// </summary>
        /// <param name="t">T.</param>
        /// <typeparam name="T">The 1st type parameter.</typeparam>
        public void ParseOne<T>(out T t)
        {
            if (!TryParseOne<T>(out t))
                throw new CommandError("A single argument expected: {0}.", typeNames[typeof(T)]);
        }

        /// <summary>
        /// Parses one value using literals
        /// </summary>
        public void ParseOne<T>(string literal1, out T t, string literal2 = null)
        {
            if (!TryParseOne<T>(literal1, out t, literal2))
                throw new CommandError("Expected syntax: {0}<{1}>{2} .",
                    (literal1 != null ? literal1 + " " : String.Empty),
                    typeNames[typeof(T)],
                    (literal2 != null ? " " + literal2 : String.Empty));
        }

        /// <summary>
        /// Parses two values
        /// </summary>
        public void ParseTwo<T, U>(out T t, out U u)
        {
            if (!TryParseTwo<T, U>(out t, out u))
                throw new CommandError("Two arguments expected: {0} and {1}.", typeNames[typeof(T)], typeNames[typeof(U)]);
        }

        /// <summary>
        /// Parses two values using literals
        /// </summary>
        public void ParseTwo<T, U>(string literal1, out T t, string literal2, out U u, string literal3 = null)
        {
            if (!TryParseTwo<T, U>(literal1, out t, literal2, out u, literal3))
                throw new CommandError("Expected syntax: {0}<{1}>{2} <{3}>{4} .",
                    (literal1 != null ? literal1 + " " : String.Empty),
                    typeNames[typeof(T)],
                    (literal2 != null ? " " + literal2 : String.Empty),
                    typeNames[typeof(U)],
                    (literal3 != null ? " " + literal3 : String.Empty));
        }

        /// <summary>
        /// Parses two values using literals
        /// </summary>
        public void ParseTwo<T, U>(out T t, string literal2, out U u, string literal3 = null)
        {
            ParseTwo(null, out t, literal2, out u, literal3);
        }

        /// <summary>
        /// Parses three values
        /// </summary>
        /// <param name="t">T.</param>
        /// <param name="u">U.</param>
        /// <param name="v">V.</param>
        /// <typeparam name="T">The 1st type parameter.</typeparam>
        /// <typeparam name="U">The 2nd type parameter.</typeparam>
        /// <typeparam name="V">The 3rd type parameter.</typeparam>
        public void ParseThree<T, U, V>(out T t, out U u, out V v)
        {
            if (!TryParseThree<T, U, V>(out t, out u, out v))
                throw new CommandError("Three arguments expected: {0}, {1} and {2}.",
                    typeNames[typeof(T)], typeNames[typeof(U)], typeNames[typeof(V)]);
        }

        /// <summary>
        /// Parses three values using literals
        /// </summary>
        public void ParseThree<T, U, V>(string literal1, out T t, string literal2, out U u, string literal3, out V v, string literal4 = null)
        {
            if (!TryParseThree<T, U, V>(literal1, out t, literal2, out u, literal3, out v, literal4))
                throw new CommandError("Expected syntax: {0}<{1}>{2} <{3}>{4} <{5}>{6} .",
                    (literal1 != null ? literal1 + " " : String.Empty),
                    typeNames[typeof(T)],
                    (literal2 != null ? " " + literal2 : String.Empty),
                    typeNames[typeof(U)],
                    (literal3 != null ? " " + literal3 : String.Empty),
                    typeNames[typeof(V)],
                    (literal4 != null ? " " + literal4 : String.Empty));
        }

        /// <summary>
        /// Parses three values using literals
        /// </summary>
        public void ParseThree<T, U, V>(out T t, string literal2, out U u, string literal3, out V v, string literal4 = null)
        {
            ParseThree(null, out t, literal2, out u, literal3, out v, literal4);
        }

        /// <summary>
        /// Parses four values
        /// </summary>
        public void ParseFour<T, U, V, W>(out T t, out U u, out V v, out W w)
        {
            if (!TryParseFour<T, U, V, W>(out t, out u, out v, out w))
                throw new CommandError("Four arguments expected: {0}, {1}, {2} and {3}.",
                    typeNames[typeof(T)], typeNames[typeof(U)], typeNames[typeof(V)], typeNames[typeof(W)]);
        }

        /// <summary>
        /// Checks if the first argument is the literal
        /// </summary>
        public bool TryPop(string literal)
        {
            if (Count < 1) return false;

            if (this[0] == literal)
            {
                this.RemoveAt(0);
                return true;
            }

            return false;
        }

        /// <summary>
        /// Tries to get a value at position 1. If successful it will be removed
        /// </summary>
        public bool TryPopOne<T>(out T t)
        {
            t = default(T);

            if (Count < 1) return false;

            if (TryParseAt(0, out t))
            {
                this.RemoveAt(0);
                return true;
            }

            return false;
        }

        /// <summary>
        /// Tries to remove one value if it is an argument, using literals.
        /// </summary>
        /// <returns><c>true</c>, if pop one was tryed, <c>false</c> otherwise.</returns>
        /// <param name="literal1">Literal1.</param>
        /// <param name="t">T.</param>
        /// <param name="literal2">Literal2.</param>
        /// <typeparam name="T">The 1st type parameter.</typeparam>
        public bool TryPopOne<T>(string literal1, out T t, string literal2 = null)
        {
            t = default(T);
            int start = (literal1 != null ? 1 : 0);
            int args = 1 + start + (literal2 != null ? 1 : 0);

            if (Count < args) return false;

            if (literal1 != null && this[0] != literal1)
                return false;

            if (literal2 != null && this[start + 1] != literal2)
                return false;

            if (TryParseAt(start, out t))
            {
                this.RemoveRange(0, args);
                return true;
            }

            return false;
        }

        /// <summary>
        /// Search through the arguments for a match, then removes both the literal and the value.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="literal"></param>
        /// <param name="t"></param>
        /// <returns></returns>
        public bool TryPopAny<T>(string literal, out T t)
        {
            t = default(T);

            for (var i = 0; i < Count; i++)
            {
                if (this[i] == literal && TryParseAt(i + 1, out t))
                {
                    RemoveRange(i, 2);
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Reconstructs the argument string
        /// </summary>
        /// <returns>A <see cref="System.String"/> that represents the current <see cref="OTA.Command.ArgumentList"/>.</returns>
        public override string ToString()
        {
            return String.Join(" ", this.Select(x => x.Replace(" ", "\\ ")).ToArray());
        }
    }

    /// <summary>
    /// 12 hour world time for use with plugins. It provides easy modifications of game time.
    /// </summary>
    public struct WorldTime
    {
        /// <summary>
        /// The maximum time possible.
        /// </summary>
        public const double TimeMax = 86400;

        /// <summary>
        /// The minimum time
        /// </summary>
        public const double TimeMin = 0;

        /// <summary>
        /// Gets or sets the hour.
        /// </summary>
        /// <value>The hour.</value>
        public byte Hour { get; set; }

        /// <summary>
        /// Gets or sets the minute.
        /// </summary>
        /// <value>The minute.</value>
        public byte Minute { get; set; }

        /// <summary>
        /// The period flag for a 12-hour clock
        /// </summary>
        /// <value><c>true</c> if A; otherwise, <c>false</c>.</value>
        public bool AM { get; set; }

        /// <summary>
        /// Translated game time version of the current instance time
        /// </summary>
        /// <value>The game time.</value>
        public double GameTime
        {
            get
            {
                var time = ((this.Hour * 60.0 * 60.0) + (this.Minute * 60.0));

                if (!this.AM && this.Hour < 12)
                    time += 12.0 * 60.0 * 60.0;
                else if (this.AM && this.Hour == 12)
                    time -= 12.0 * 60.0 * 60.0;

                time -= 4.5 * 60.0 * 60.0;

                if (time < 0) time = TimeMax + time;

                return time;
            }
        }

        /// <summary>
        /// Parses time in the format of HH:mm[am|pm]
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static WorldTime? Parse(string input)
        {
            var split = input.Split(':');
            if (split.Length == 2)
            {
                byte hour, minute;
                if (Byte.TryParse(split[0], out hour) && split[1].Length > 2 && hour < 13)
                {
                    if (Byte.TryParse(split[1].Substring(0, split[1].Length - 2), out minute) && minute < 60)
                    {
                        var tk = split[1].Remove(0, split[1].Length - 2);

                        return new WorldTime()
                        {
                            Hour = hour,
                            Minute = minute,
                            AM = tk.ToLower() == "am"
                        };
                    }
                }
            }
            return null;
        }

        /// <summary>
        /// Parses game time to a WorldTime instance
        /// </summary>
        /// <param name="time">Time.</param>
        public static WorldTime? Parse(double time)
        {
            time += 4.5 * 60.0 * 60.0;

            if (time > TimeMax) time = time - TimeMax;

            bool am = time < 12.0 * 60.0 * 60.0 || time == TimeMax;
            if (!am) time -= 12.0 * 60.0 * 60.0;

            var hour = (int)(time / 60.0 / 60.0);
            var min = (int)((time - (hour * 60.0 * 60.0)) / 60.0);

            if (hour == 0) hour = 12;
            if (hour > 12) hour -= 12;

            return new WorldTime()
            {
                Hour = (byte)hour,
                Minute = (byte)min,
                AM = am
            };
        }

        /// <summary>
        /// Formats to 12-hour time
        /// </summary>
        /// <returns>A <see cref="System.String"/> that represents the current <see cref="OTA.Command.WorldTime"/>.</returns>
        public override string ToString()
        {
            return String.Format("{0}:{1:00} {2}", Hour, Minute, AM ? "AM" : "PM");
        }

        #if true
        public static bool Test()
        {
            if ((new WorldTime()
            {
                Hour = 4,
                Minute = 30,
                AM = true
            }).ToString() != "4:30 AM")
                return false;

            if (WorldTime.Parse(0).ToString() != "4:30 AM")
                return false;

            if (WorldTime.Parse("12:00pm").Value.GameTime != 27000) //43200.0)
                return false;
            if (WorldTime.Parse("12:00am").Value.GameTime != WorldTime.TimeMax - (4.5 * 60.0 * 60.0)) //43200.0)
            {
                var aa = WorldTime.Parse("12:00am").Value.GameTime;
                var a = WorldTime.Parse("12:30am").Value.GameTime;
                var ab = WorldTime.Parse("12:59am").Value.GameTime;
                var b = WorldTime.Parse("1:00am").Value.GameTime;
                var c = WorldTime.Parse("3:00am").Value.GameTime;
                var d = WorldTime.Parse("4:00am").Value.GameTime;
                var f = WorldTime.Parse("5:00am").Value.GameTime;
                return false;
            }

            var _12 = WorldTime.Parse("12:00am").Value.GameTime;
            var parsed = WorldTime.Parse(_12);
            if (parsed.ToString() != "12:00 AM")
            {
                return false;
            }

            _12 = WorldTime.Parse("12:01am").Value.GameTime;
            parsed = WorldTime.Parse(_12);
            if (parsed.ToString() != "12:01 AM")
            {
                return false;
            }

            for (var h = 1; h <= 24; h++)
            {
                for (var m = 0; m < 60; m++)
                {
                    var time = String.Format("{0}:{1:00} {2}", h > 12 ? h - 12 : h, m, h < 12 ? "AM" : "PM");

                    System.Diagnostics.Debug.WriteLine("Testing time " + time);
                    var t = Parse(time);
                    if (t.ToString() != time)
                    {
                        return false;
                    }

                    var t2 = Parse(t.Value.GameTime);
                    if (t2.ToString() != time)
                    {
                        Parse(t.Value.GameTime);
                        Parse(t.Value.GameTime);
                        return false;
                    }

                    //if (t2.ToString() == (new WorldTime()
                    //{
                    //    Hour = (byte)h,
                    //    Minute = (byte)m,
                    //    AM = h <= 12
                    //}).ToString())
                    //{
                    //    return false;
                    //}
                }
            }

            return true;
        }
        #endif
    }
}