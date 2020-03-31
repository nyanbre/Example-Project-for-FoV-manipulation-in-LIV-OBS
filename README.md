# Example Project for FoV manipulation in LIV+OBS

## Description

[LIV](https://liv.tv/) and games that use LIV SDK don't support smooth FoV update at the moment, so I came up with a crude workaround for that.
It makes [OBS](https://obsproject.com/) resize LIV source window synchronously with data that LIV camera plugin is feeding.

## See it in action 
(not a perfect shot though, it should be much better for other cases)
![](capture.gif)

## Installation

Aside from **a camera plugin with a tcplistener** itself you have to add `obs_fov_effects_script.py` script to *OBS*: *[Tools] -> [Scripts]*

### BUT! Before doing that:

1) Make sure your *LIV source* is named `"LIV"` or something that **contains** `"LIV"`
2) Make sure **none of the other names** (sources AND SCENES) contain `"LIV"`
3) **Edit Transform...** of *LIV source* so that [Positional Alignment] will be [Center]
4) And then install the `obs_fov_effects_script.py` script

If it doesn't work, try installing Python 3.6 in "for all users" mode and making OBS use it in [Tools] -> [Scripts] window.

*If you have any questions, feel free to DM me at discord: `Nyanbre#1578`*  