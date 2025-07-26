function sanitizeInputValue(value) {
	const accentMap = {
		'á': 'a', 'à': 'a', 'ä': 'a', 'â': 'a', 'ã': 'a',
		'é': 'e', 'è': 'e', 'ë': 'e', 'ê': 'e',
		'í': 'i', 'ì': 'i', 'ï': 'i', 'î': 'i',
		'ó': 'o', 'ò': 'o', 'ö': 'o', 'ô': 'o', 'õ': 'o',
		'ú': 'u', 'ù': 'u', 'ü': 'u', 'û': 'u',
		'ñ': 'ñ', 'ç': 'c',

		'Á': 'A', 'À': 'A', 'Ä': 'A', 'Â': 'A', 'Ã': 'A',
		'É': 'E', 'È': 'E', 'Ë': 'E', 'Ê': 'E',
		'Í': 'I', 'Ì': 'I', 'Ï': 'I', 'Î': 'I',
		'Ó': 'O', 'Ò': 'O', 'Ö': 'O', 'Ô': 'O', 'Õ': 'O',
		'Ú': 'U', 'Ù': 'U', 'Ü': 'U', 'Û': 'U',
		'Ñ': 'Ñ', 'Ç': 'C'
	};

	const stripped = value
		.split('')
		.map(char => accentMap[char] || char)
		.join('')
		.replace(/[^a-zA-ZñÑ0-9\s]/g, '');

	return stripped.toUpperCase();
}