using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using com.brg.Common;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.AddressableAssets;
using Random = System.Random;

namespace com.tinycastle.SeatSeekers
{
    /// <summary>
    /// Class representing data of a level, as well as serializing and deserializing methods to strings.
    /// </summary>
    [ExposeRead(typeof(GameDataManager))]
    [ReadableMap(typeof(LevelDataReader), "Id")]
    public partial class LevelData
    {
        private const char TOKEN_SEPARATOR = '.';
        private const char ARRAY_SEPARATOR = '_';

        public int Width { get; private set;}

        public int Height { get; private set; }

        public int[] Grid { get; private set; }

        public bool Playable => Grid != null && Grid.Length > 0;

        public LevelData(int[] grid, int width, int height)
        {
            Grid = grid;
            Width = width;
            Height = height;
        }

        public LevelData(LevelData other)
        {
            Grid = other.Grid.ToArray();
            Width = other.Width;
            Height = other.Height;
        }

        public void ExpandOneLaneLeft()
        {
            var newGrid = new int[(Width + 1) * Height];
            for (int x = 0; x < Width; ++x)
            {
                for (int y = 0; y < Height; ++y)
                {
                    var i = x + y * Width;
                    var newI = x + 1 * Width;
                    newGrid[newI] = Grid[i];
                }
            }

            Grid = newGrid;
            Width += 1;
        }
                
        /// <summary>
        /// Assuming correct grid data, return the seat at the position (x,y).
        /// <para>If seat at the position is double and on the right, the return coordinate in the result <see cref="SeatData"/> will be (x - 1, y)</para>
        /// </summary>
        /// <param name="x">X coordinate</param>
        /// <param name="y">Y coordinate</param>
        /// <returns>A <see cref="SeatData"/> object with the seat's data. Will return <see cref="SeatData.Invalid"/> if out of bounds.</returns>
        public SeatData GetSeatAt(int x, int y)
        {
            if (x < 0 || y < 0) return SeatData.Invalid;
            var i = x + y * Width;
            if (i >= Grid.Length) return SeatData.Invalid;

            var value = Grid[i];
            var color = value % SeatData.DOUBLE_SEAT_LEFT;
            if (SeatData.CheckDoubleLeft(value))
            {
                // Require right exist
                var nextI = x + 1 + y * Width;
                if (IsLastCellOfRow(x) || SeatData.CheckDoubleRight(Grid[nextI]))
                {
                    // Demote to normal seat
                    value %= SeatData.DOUBLE_SEAT_LEFT;
                    LogObj.Default.Warn($"Level Data has invalid double left seat at ({x},{y}).");
                }
                else
                {
                    var nextValue = Grid[nextI];
                    var nextColor = nextValue % SeatData.DOUBLE_SEAT_LEFT;
                    if (nextColor != color)
                    {
                        // Demote to normal seat
                        value %= SeatData.DOUBLE_SEAT_LEFT;
                        LogObj.Default.Warn($"Level Data has mismatching double left and right seat color at ({x},{y}) and ({x + 1},{y}).");
                    }
                }
            }
            else if (SeatData.CheckDoubleRight(value))
            {
                // Require left exist
                var lastI = x - 1 + y * Width;
                if (x == 0 || SeatData.CheckDoubleLeft(Grid[lastI]))
                {
                    // Demote to normal seat
                    value %= SeatData.DOUBLE_SEAT_LEFT;
                    LogObj.Default.Warn($"Level Data has invalid double right seat at ({x},{y}).");
                }
                else
                {
                    var lastValue = Grid[lastI];
                    var lastColor = lastValue % SeatData.DOUBLE_SEAT_LEFT;
                    if (lastColor != color)
                    {
                        // Demote to normal seat
                        value %= SeatData.DOUBLE_SEAT_LEFT;
                        LogObj.Default.Warn($"Level Data has mismatching double left and right seat color at ({x},{y}) and ({x + 1},{y}).");
                    }
                    else
                    {
                        // Will return left seat.
                        x -= 1;
                        value = lastValue;
                    }
                }
            }

            return new SeatData()
            {
                X = x,
                Y = y,
                Value = value
            };
        }

        public int[] GetQueuers()
        {
            var queuers = new int[SeatData.MAX_COLOR];
            var anyColorSeatCount = 0;
            foreach (var seat in IterateAllSeats())
            {
                var color = seat.Color;
                var addCount = seat.IsDouble ? 2 : 1;
                if (color == (int)SeatEnum.ANY) anyColorSeatCount += addCount;
                else if (color is >= (int)SeatEnum.BLUE and <= (int)SeatEnum.BROWN)
                {
                    queuers[color - (int)SeatEnum.BLUE] += addCount;
                }
            }
            
            for (var i = SeatData.MAX_COLOR - 1; i >= 0 ; --i)
            {
                if (queuers[i] > 0)
                {
                    queuers[i] += anyColorSeatCount;
                    anyColorSeatCount = 0;
                    break;
                }
            }

            // In case there are no explicit customers
            queuers[0] += anyColorSeatCount;

            return queuers;
        }

        public bool RemoveSeat(int x, int y)
        {
            if (x < 0 || x >= Width || y < 0 || y >= Height) return false;

            var currentSeat = GetSeatAt(x, y);
            var i = currentSeat.X + currentSeat.Y * Width;
            Grid[i] = (int)SeatEnum.NONE;
            if (currentSeat.IsDouble && !IsLastCellOfRow(currentSeat.X)) Grid[currentSeat.X] = (int)SeatEnum.NONE;
            return true;
        }

        public bool SetSeat(SeatData newSeat)
        {
            if (newSeat.X < 0 || newSeat.X >= Width || newSeat.Y < 0 || newSeat.Y >= Height) return false;
            if (IsLastCellOfRow(newSeat.X) && newSeat.IsDouble) return false;

            RemoveSeat(newSeat.X, newSeat.Y);
            RemoveSeat(newSeat.X + 1, newSeat.Y);

            var i = newSeat.X + newSeat.Y * Width;
            
            Grid[i] = newSeat.Color + SeatData.DOUBLE_SEAT_LEFT;
            if (newSeat.IsDouble) Grid[i + 1] = newSeat.Color + SeatData.DOUBLE_SEAT_RIGHT;
            
            return true;
        }

        public bool InitializeGridToEmpty(int w, int h)
        {
            if (Playable) return false;
            Width = w;
            Height = h;
            Grid = new int[Width * Height];
            return true;
        }

        public IEnumerable<SeatData> IterateAllSeats()
        {
            for (int i = 0; i < Grid.Length; ++i)
            {
                var x = i % Width;
                var y = i / Width;
                var value = Grid[i];

                if (SeatData.CheckDoubleLeft(value) && !IsLastCellOfRow(x))
                {
                    // Skip the next, since it's a right seat data
                    i += 1;
                }
                else if (SeatData.CheckDoubleLeft(value))
                {
                    // Wrong data, return correct data
                    value = Grid[i] % SeatData.DOUBLE_SEAT_LEFT;
                }
                else if (SeatData.CheckDoubleRight(value))
                {
                    // Should not reach a double right, return a single seat instead
                    value = Grid[i] % SeatData.DOUBLE_SEAT_LEFT;
                }

                yield return new SeatData()
                {
                    X = x,
                    Y = y,
                    Value = value,
                };
            }
        }

        private void ValidateSeats()
        {
            for (int i = 0; i < Grid.Length; ++i)
            {
                var x = i % Width;
                var y = i / Width;
                var value = Grid[i];
                if (value % SeatData.DOUBLE_SEAT_LEFT == 0) Grid[i] = (int)SeatEnum.NONE;
                else if (SeatData.CheckDoubleLeft(value) && IsLastCellOfRow(x))
                {
                    // Demote to single seat
                    Grid[i] = value % SeatData.DOUBLE_SEAT_LEFT;
                }
                else if (SeatData.CheckDoubleLeft(value))
                {
                    // Require next is double right
                    var nextValue = Grid[i + 1];
                    if (SeatData.CheckDoubleRight(nextValue) && SeatData.CheckSameColor(value, nextValue))
                    {
                        // Valid seat, skip the next index
                        i += 1;
                    }
                    else
                    {
                        // Not valid seat, will check next seat, for now, demote the current to single seat
                        Grid[i] = value % SeatData.DOUBLE_SEAT_LEFT;
                    }
                }
                else if (SeatData.CheckDoubleRight(value))
                {
                    // Should not reach a double right seat, automatically demote
                    Grid[i] = value % SeatData.DOUBLE_SEAT_LEFT;
                }
                
                // Otherwise, it's a single seat.
            }
        }

        private bool IsLastCellOfRow(SeatData data)
        {
            return IsLastCellOfRow(data.X);
        }
        
        private bool IsLastCellOfRow(int x)
        {
            return (x + 1) % Width == 0;
        }

        public void FillDoorSeat(int genColorCount)
        {
            if (!Playable) return;
            if (Grid[Width - 1] == (int)SeatEnum.NONE || Grid[Width - 1] == (int)SeatEnum.ANY)
            {
                Grid[Width - 1] = (int)SeatEnum.BLUE + UnityEngine.Random.Range(0, genColorCount);
            }
        }
        
        public string MakeString()
        {
            var builder = new StringBuilder();
            builder.Append(Width);
            builder.Append(TOKEN_SEPARATOR);
            builder.Append(Height);
            builder.Append(TOKEN_SEPARATOR);

            foreach (var num in Grid)
            {
                builder.Append(num);
                builder.Append(ARRAY_SEPARATOR);
            }

            builder.Remove(builder.Length - 1, 1);
            return builder.ToString();
        }
        
        public static LevelData Make(string input)
        {
            // Input format: "<width>,<height>,<seat_elements>
            var tokens = input.Split(TOKEN_SEPARATOR, StringSplitOptions.RemoveEmptyEntries);

            if (tokens.Length != 3)
            {
                LogObj.Default.Error($"Cannot parse level input {input}, will return an empty level");
                return new LevelData(Array.Empty<int>(), 0, 0);
            }

            var validW = int.TryParse(tokens[0], out var width);
            var validH = int.TryParse(tokens[1], out var height);

            if (!validW || !validH || width <= 0 || height <= 0)
            {
                LogObj.Default.Error($"Cannot parse level input {input} because either height or width is invalid, will return an empty level");
                return new LevelData(Array.Empty<int>(), 0, 0);
            }
            
            // Parse the grid
            var totalCount = width * height;
            var subTokens = tokens[2].Split(ARRAY_SEPARATOR, StringSplitOptions.RemoveEmptyEntries);
            var subTokenLength = subTokens.Length;
            var minLength = Math.Min(totalCount, subTokenLength);

            if (totalCount < subTokenLength)
            {
                LogObj.Default.Warn($"Level input has MORE seat tokens than the capacity of the dimension " +
                                    $"(Tokens: {subTokenLength}, Max: {width}X{height}={totalCount}");
            }
            else if (totalCount > subTokenLength)
            {
                LogObj.Default.Warn($"Level input has FEWER seat tokens than the capacity of the dimension " +
                                    $"(Tokens: {subTokenLength}, Max: {width}X{height}={totalCount}");
            }

            var array = new int[totalCount];
            for (var i = 0; i < minLength; ++i)
            {
                var seatToken = subTokens[i];
                var validToken = int.TryParse(seatToken, out var value);
                if (!validToken)
                {
                    LogObj.Default.Warn($"Cannot parse input seat tokens at index {i} (value: \"{seatToken}\"");
                    continue;
                }

                array[i] = value;
            }

            var newLevel = new LevelData(array, width, height);
            newLevel.ValidateSeats();
            
            return newLevel;
        }
    }

    public class LevelDataReader : IMapReader<LevelData>
    {
        private readonly string _addressableKey;
        private Dictionary<string, LevelGen> _data;
        private bool _runningTask;

        public Dictionary<string, LevelData> AllData => _data.ToDictionary(x => x.Key, x => x.Value.LevelData);
        
        public LevelDataReader()
        {
            _addressableKey = "jsons/level_inputs.csv";
            _runningTask = false;
            _data = null;
        }
        
        public async Task<bool> ReadDataAsync()
        {
            var oldData = _data;
            if (_runningTask) return false;

            _runningTask = true;
            try
            {
                var handle = Addressables.LoadAssetAsync<TextAsset>(_addressableKey);
                var task = handle.Task;
                await task;
                
                var levelInputText = handle.Result.text.Split('\n', StringSplitOptions.RemoveEmptyEntries);
                var levelInputTask = Task.Run(() => Utils.ParseCsv(levelInputText));
                var levelGens = await levelInputTask;
                
                _data = levelGens?.ToDictionary(x => x.LevelName, x => x) ?? throw new Exception("Parsed data is null, this is not allowed");
                _runningTask = false;
                return true;
            }
            catch (Exception e)
            {
                LogObj.Default.Warn($"LevelDataReader cannot read data at \"{_addressableKey}\". Exception: {e}");
                _runningTask = false;
                _data = oldData;
                return false;
            }
        }

        public bool ReadData()
        {
            throw new NotImplementedException();
        }

        public LevelData GetItem(string key)
        {
            return _data[key].LevelData;
        }

        public LevelData GetNextItem(string key)
        {
            LogObj.Default.Warn("Cannot get next via LevelData, use LevelEntry instead.");
            return null;
        }

        public LevelData GetPrevItem(string key)
        {
            LogObj.Default.Warn("Cannot get prev via LevelData, use LevelEntry instead.");
            return null;
        }
    }
}