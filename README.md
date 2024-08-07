# Honkai Star Rail simulator
> [!CAUTION]
> At the moment, refactoring is underway, and there is a fairly significant change in the architecture. After fixing multithreading, formulas and autotests, SDK will be available for developers. JetBrains RIDER motivated me to switch from Windows Forms to WPF. And for one thing, I decided to redo all the calculations in the simulator... in general, I decided to redo everything

## Description
### What is it?

This is combat simulator reproducing combat in the Honkai Star Rail game with 100% accuracy.

### What is can to do with it?
- grab data from official API. Get your characters and equipment by UID and save it. Use it in sim scenarios![изображение](https://github.com/YakudzaKY/HSR_SIM/assets/110550702/d9c98881-9136-4f88-b02c-614527654b7d)
- single simmulation for debug characters or research "how it calculated?"![изображение](https://github.com/YakudzaKY/HSR_SIM/assets/110550702/ce664f2b-c6ee-4ad0-b1b6-53e801175b8a)
- profiles comparison ![изображение](https://github.com/YakudzaKY/HSR_SIM/assets/110550702/80eadeb5-5b80-4301-a29d-3b7e18f1715e)
- gear(stats) replacment calc with OCR(parse stats from game window) ![изображение](https://github.com/YakudzaKY/HSR_SIM/assets/110550702/5833ec15-dc22-4b97-be72-a4a608d00df0)


### What profit i can get(at least when the project is in alpha)?
- test the character,LC,gear before official release
- optimal build your characters
- find best composition for every case

## Developers node
### Poject sctructure
* HSR_SIM_CLIENT
> [!TIP]
> Start project use from here.
Its Ui for SIM all functionality starts from here

* HSR_SIM_CONTENT
> [!TIP]
> free developers are needed here
content for the simulator. Characters, cones, opponents, equipment. Horizontal development takes place here

* HSR_SIM_LIB
  
The filling of the whole project. All mechanics, simulations and calculations are here. Btw you can write your own UI for this lib




## Plans/Features:
- SDK documentation
- transfer the entire application to WPF
- repair multi threading simmulation
- Need some content(more characters)
- serious optimization of calculation of the result of formulas. Cause calculations moved from native c# methods into dynamic formulas(for better accuracy and debug). performance dropped by a factor of 20. A buffer was developed as an optimization measure, but further measures must be taken
- assessment of the level of current equipment
- Character research(find best relic set with average stats)

# License
This project is licensed under the terms of the MIT license.

## Developed in [JetBrains Rider](https://www.jetbrains.com/community/opensource/?utm_campaign=opensource&utm_content=approved&utm_medium=email&utm_source=newsletter&utm_term=jblogo#support)

![JetBrains](https://github.com/YakudzaKY/HSR_SIM/assets/110550702/2a32b7fc-2df1-4284-a192-884599aac920)

