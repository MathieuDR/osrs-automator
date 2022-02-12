namespace DiscordBot.Helpers.Extensions; 

public static class NumberParser {
	// parse a string into a long
	// where K and M stands for thousands and millions
	// the k and m can be appended with or without space
	// the number can be negative
	public static long ToLong(this string input) {
		decimal result = 0;
		// removes spaces in input
		input = input.Replace(" ", "");
		// lower input
		input = input.ToLower();
		
		//	parse number
		if (input.Contains("k")) {
			// remove k
			input = input.Replace("k", "");
			// parse number
			result = decimal.Parse(input) * 1000;
		} else if (input.Contains("m")) {
			// remove m
			input = input.Replace("m", "");
			// parse number
			result = decimal.Parse(input) * 1000000;
		} else {
			// parse number
			result = decimal.Parse(input);
		}
		
		// convert decimal to long
		return (long)result;
	}
}
