Chia-NFT-Minter is a utility to generate metadata, collection, rpcs, offerings and to bulkmint nfts

Quickstart:
1. Create a folder for your NFT Project.
2. Place `Minter-UI.exe` in it and execute it:
![image](https://user-images.githubusercontent.com/117320700/205461679-4bbe5f16-3999-4649-a5a5-43c01e41b894.png)

3. Execute `Minter-UI.exe`. You will notice, a new project is set up automatically:
![image](https://user-images.githubusercontent.com/117320700/205461699-92f9c15c-0667-4ad1-8920-68aebe32405b.png)

4. replace `logo.png` and `banner.png` with collection logo and banner. Supported formats are `.jpg`and `.png` `.webm` might work but is untested.
the recommended aspect ratio for banner is 2:1

5. place your nft images/documents in the final folder
![image](https://user-images.githubusercontent.com/117320700/205461905-11adbacc-d334-4992-9d48-0b5833de68b7.png)

6. restart the application. Edit Collection information and hit safe. Depending on internet connection speed, it may take a little while to upload logo and banner to nft.storage
![image](https://user-images.githubusercontent.com/117320700/205461888-673349a1-8ca5-4b18-b938-f4e1c1033a8f.png)

7. Edit metadata in the metadata tab. If you already have json metadata files with chip-0007 std, place them in the metadata folder. the name must be the same as the mages, except the file ending (`.json`)
![image](https://user-images.githubusercontent.com/117320700/205461922-4f78b6a9-2d70-4e5a-b4a2-04fbaf1627df.png)

8. Head over to the minting tab and press mint. In case you already have minted nfts but no .rpc files for them, you can copy the existing metadata files to the rpc folder. The given nfts will then be excluded.
![image](https://user-images.githubusercontent.com/117320700/205461960-6e392402-9c57-4fe1-b8d6-7fc568e05364.png)
