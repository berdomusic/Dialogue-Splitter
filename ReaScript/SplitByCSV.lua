local function trim(s)
    return s:match("^%s*(.-)%s*$")
end

-- Robust CSV parser (handles quoted commas)
local function parse_csv_line(line)
    local res = {}
    local pos = 1
    local len = #line
    while pos <= len do
        if line:sub(pos,pos) == '"' then
            local cpos = pos + 1
            local val = ""
            while true do
                local next_quote = line:find('"', cpos)
                if not next_quote then break end
                val = val .. line:sub(cpos, next_quote-1)
                cpos = next_quote + 1
                if line:sub(cpos,cpos) ~= '"' then break end
                val = val .. '"'
            end
            table.insert(res, val)
            pos = line:find(",", cpos) or (len + 1)
            pos = pos + 1
        else
            local next_comma = line:find(",", pos) or (len + 1)
            table.insert(res, trim(line:sub(pos, next_comma-1)))
            pos = next_comma + 1
        end
    end
    return res
end

-- =========================
-- REMEMBER LAST CSV FOLDER
-- =========================
local last_csv_folder = reaper.GetExtState("CSV_VO_IMPORT", "last_folder") or ""
local retval, csv_path = reaper.GetUserFileNameForRead(last_csv_folder, "Select CSV file", ".csv")
if not retval then return end

-- save folder for next time
local folder_path = csv_path:match("^(.*[\\/])")
if folder_path then
    reaper.SetExtState("CSV_VO_IMPORT", "last_folder", folder_path, true)
end

-- =========================
-- SELECTED ITEM
-- =========================
local item = reaper.GetSelectedMediaItem(0, 0)
if not item then
    reaper.ShowMessageBox("Select a source item!", "Error", 0)
    return
end

local take = reaper.GetActiveTake(item)
if not take then return end
local source = reaper.GetMediaItemTake_Source(take)
local item_pos = reaper.GetMediaItemInfo_Value(item, "D_POSITION")

-- =========================
-- TRACK BELOW
-- =========================
local track = reaper.GetMediaItemTrack(item)
local track_idx = math.floor(reaper.GetMediaTrackInfo_Value(track, "IP_TRACKNUMBER"))
if reaper.CountTracks(0) <= track_idx then
    reaper.InsertTrackAtIndex(track_idx, true)
end
local dest_track = reaper.GetTrack(0, track_idx)

-- =========================
-- TIME PARSER
-- =========================
local function time_to_sec(t)
    local m, s = t:match("(%d+):(%d+%.%d+)")
    return tonumber(m) * 60 + tonumber(s)
end

-- =========================
-- MAIN LOOP
-- =========================
local created = 0
reaper.Undo_BeginBlock()

for line in io.lines(csv_path) do
    if not line:find("Source Audio File") then
        local cols = parse_csv_line(line)
        local start_sec = time_to_sec(cols[2])
        local end_sec   = time_to_sec(cols[3])
        if start_sec and end_sec then
            local length = end_sec - start_sec
            local new_pos = item_pos + start_sec
            local item_name = cols[4] or "slice"

            -- CREATE ITEM
            local new_item = reaper.AddMediaItemToTrack(dest_track)
            reaper.SetMediaItemInfo_Value(new_item, "D_POSITION", new_pos)
            reaper.SetMediaItemInfo_Value(new_item, "D_LENGTH", length)

            local new_take = reaper.AddTakeToMediaItem(new_item)
            reaper.SetMediaItemTake_Source(new_take, source)
            reaper.SetMediaItemTakeInfo_Value(new_take, "D_STARTOFFS", start_sec)
            reaper.GetSetMediaItemTakeInfo_String(new_take, "P_NAME", item_name, true)

            created = created + 1
        end
    end
end

reaper.UpdateArrange()
reaper.Undo_EndBlock("Create slices below from CSV", -1)
reaper.ShowMessageBox("Created " .. created .. " items on track below.", "Done", 0)

