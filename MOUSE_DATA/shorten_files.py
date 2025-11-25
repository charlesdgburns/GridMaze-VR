import pandas as pd
from pathlib import Path

import numpy as np
from scipy.ndimage import gaussian_filter1d

TEST_DIR = Path('./')

def shorten_file(session_dir, start_frame = 10000, n_frames = 900):
    '''Input session Path object with the following:
    frames.trajectories.htsv
    frames.trialInfo.htsv'''
    traj_df = pd.read_csv(session_dir/'frames.trajectories.htsv', sep='\t')
    trialinfo_df = pd.read_csv(session_dir/'frames.trialInfo.htsv', sep = '\t')

    short_traj_df = traj_df.iloc[start_frame:start_frame+n_frames]
    #add smoothing to test
    short_traj_df['head_direction.value'] = circular_gaussian_smooth(short_traj_df['head_direction.value'].values)
    
    short_traj_df.to_csv(session_dir/'short.frames.trajectories.htsv',sep='\t')
    
    short_info_df = trialinfo_df.iloc[start_frame:start_frame + n_frames]
    short_info_df.to_csv(session_dir/'short.frames.trialInfo.htsv', sep='\t')
    print(f'saved out shortened data for {session_dir}')
    return short_traj_df, short_info_df



def circular_gaussian_smooth(data: np.array, sigma: float = 1.0) -> pd.Series:
    """
    Performs 1D Gaussian smoothing on circular data (degrees, 0 to 360).

    The function uses the sine and cosine transformation method to correctly 
    handle the wrap-around boundary condition (360 degrees back to 0).

    Args:
        data: A pandas Series containing angular data in degrees (0 to 360).
        sigma: The standard deviation (sigma) of the Gaussian kernel, 
               which controls the amount of smoothing.

    Returns:
        A pandas Series containing the smoothed angular data in degrees (0 to 360).
    """
    data_rad = np.deg2rad(data)
    sin_data = np.sin(data_rad)
    cos_data = np.cos(data_rad)
    sin_smooth = gaussian_filter1d(sin_data, sigma, mode='wrap')
    cos_smooth = gaussian_filter1d(cos_data, sigma, mode='wrap')
    smoothed_rad = np.arctan2(sin_smooth, cos_smooth)
    smoothed_deg = np.rad2deg(smoothed_rad)
    smoothed_deg_wrapped = (smoothed_deg + 360) % 360
    return smoothed_deg_wrapped

shorten_file(TEST_DIR)