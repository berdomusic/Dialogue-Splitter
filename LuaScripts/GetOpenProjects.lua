-- GetOpenProjects.lua
-- Script to get all open Reaper projects and their tracks

local function GetOpenProjects()
	local projects = {}
	local projCount = 0
	local proj = reaper.EnumProjects(projCount)

	while proj do
		local projName = reaper.GetProjectName(proj)
		local projPath = reaper.GetProjectPath(proj)
		local projFile = reaper.GetProjectFileName(proj)

		-- Get tracks from this project
		local tracks = {}
		local trackCount = reaper.CountTracks(proj)

		for i = 0, trackCount - 1 do
			local track = reaper.GetTrack(proj, i)
			local _, trackName = reaper.GetTrackName(track, "")
			if trackName == "" then
				trackName = "Track " .. (i + 1)
			end
			table.insert(tracks, trackName)
		end

		table.insert(projects, {
			name = projName,
			path = projPath,
			file = projFile,
			fullPath = projPath .. "/" .. projFile,
			tracks = tracks
		})

		projCount = projCount + 1
		proj = reaper.EnumProjects(projCount)
	end

	return projects
end

-- Output to console
local projects = GetOpenProjects()
reaper.ShowConsoleMsg("REAPER_PROJECTS_START\n")
reaper.ShowConsoleMsg(reaper.JSON_Encode(projects))
reaper.ShowConsoleMsg("\nREAPER_PROJECTS_END\n")