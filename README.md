# C5FileTransfer

Siemens Comfort panel file transfer via HTTP

Via the built web server of the HMI it is possible to download files from the HMI to a computer.
No software change is required for the TIA project. The HMI web server has to be configured. The service has to be started and furthermore a web-user must exist with a password.
Then using a web browser, the files can be downloaded.

<i>Proposing a tool for the solution</i>: C5FileTransfer

To automate these steps, a tool is proposed that downloads the required files from the HMI to the host computer.
The command-line tool can be time-triggered (e.g. scheduled task) or on-demand to initiate the download if the required files from the HMI.

