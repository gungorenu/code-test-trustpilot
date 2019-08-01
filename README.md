# trustpilot
my super lazy attempt on trustpilot code challenge (e4820b45d2277f3844eac66c903e84be, 23170acc097c24edb98fc5488ab033fe and 665e5bcb0c20062fe8abaaf4628bb154)

## algorithm in general
every word is an anagram for the app, those anagrams will be identified by an integer (18bit integer is enough). the phrase is changed to **ailnooprssttttuuyw** and every letter is represented by a bit. 
*Word Power*: word power is length of word actually, it corresponds to number of bits set in a word code.

*app works in 2 phases*: first find out 3 or 4 word anagrams that make up a sentence, then check for every word that makes those sub-anagrams and calculate hash of the sentence. app supports 3 or 4 word anagrams, for more than 4 words, it requires to be adapted again but algorithm will stay in overall. app does not list or store all anagrams, not necessary
difficulty of a phrase depends on word count, 2 word is easy, 3 word is medium, 4 word is hard and so on...
some code is hardcoded for performance, especially in bit calculation, not required though

## 3 word algorithm:
* words are A B C
* we pick A first from word anagram 'WordCode (int)' list with a power of min 6, p(X) means length of X (18/3 so one word must be at least 6 characters)
* we pick B for every A and form a small tree with one root (A) and possible leaves (B), we use WordCode, the integer representation of word anagrams
* p(A) >= p(B) >= Math.Ceiling( ( 18 - p(A) ) /2 )
* we do not pick C, not necessary, instead we check if specific A+B combination allow us to get a word C, algorithm removes unnecessary pairs of A+B
* finally we can work on sentences now, with nested foreachs (I dont like but wont bother to fix) we brute force every sentence possible with the word anagrams, 2 answers can be found like this
  
## 4 word algorithm
* similar to 3 word algorithm but less checks
* we pick A first from word anagram 'WordCode (int)' list with a length of min 5 (18/4 so one word must be at least 5 characters)
* we pick B for every A and form a small tree with one root (A) and possible leaves (B), we use WordCode, the integer representation of word anagrams
* p(A) >= p(B) >= Math.Ceiling( ( 18 - p(A) ) /3 )
* we proceed to pick a word C like we did for word B, p(B) >= p(C) >= Math.Ceiling( ( 18 - ( p(A) + p(B) ) ) /2 )
* at this point we could find 3 word answers as well, I did not go that way, see notes below for reason
* we do not pick D, not necessary, instead we check if specific A+B+C combination allow us to get a word D
* finally we can work on sentences now, with nested foreachs (I dont like but wont bother to fix) we brute force every sentence possible with the word anagrams, 1 answer can be found like this
  
## problems & limitations:
* does not work for 5+ word sentences, needs adaptation if requested
* for faster performance a lot of code is hardcoded, of course can be changed to make generic but necessary?
* algorithm will not work on anagrams longer than 32 chars, can be adapted to 64 easily but longer than that requires (custom) bitset handlers
* **parallelism** of course, it is possible but I did not think of this on this task since it was able to find stuff within 5 minutes. it is my bad for designing for parallel run: 
  *history*: found first two and sent app, I knew third was 4 word anagram and did not go for it. trustpilot guys asked me to find the third (which was not mentioned that it was required). I added a solution for the third but due to my lazyness I skipped parallelism. embarrassing for me. 2-3 words are pretty fast but 4 word one (and future 5+ words) can be parallelised.
## final notes
...and finally, I believe there is an easier and better way to find the answers. maybe with search based modules but I have no experience on them and their abilities (at the time I found the results).
