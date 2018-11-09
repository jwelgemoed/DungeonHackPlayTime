# DungeonHackPlayTime
A space for me to play around with Directx and 3d programming in general.
WARNING!! Here you will definitely not find clean code, pretty patterns or other nice things :)

This is repo is entirely meant for me to play around with DirectX and other graphics stuff that I find interesting.

That said the following features have been implemented:

1) Maps/Mazes are generated randomly by using a growing tree algorithm (http://weblog.jamisbuck.org/2011/1/27/maze-generation-growing-tree-algorithm#)
2) Normal, Bump and Specular mapping have been implemented.
3) Directional, Point and Spot lights are supported.
4) World Space partitioning is done via QuadTree partitioning (to be upgraded to Octtree at later date)
5) Quad tree is rendered in parallel using deferred rendering.
6) Occlussion culling (CPU side) is done via a software based rasterizer and depth buffer. Done only for occluder meshes (walls, floors, ceilings).
7) Fustrum culling.

Todo:
1) Switch to deferred shading for better lighting/post processing possibilities.
2) Switch to DirectX12


MAIN DEVELOPMENT BRANCH IS /dev
