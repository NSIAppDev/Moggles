module.exports = {
    extends: [
        // add more generic rulesets here, such as:
        'eslint:recommended',
        'plugin:vue/recommended'
    ],
    env: {
        "node": true
    },
    rules: {
        // override/add rules settings here, such as:
        // 'vue/no-unused-vars': 'error'
        "vue/max-attributes-per-line": ["warn", {
            "singleline": 3,
            "multiline": {
                "max": 3,
                "allowFirstLine": true
            }
        }],
        "vue/html-closing-bracket-newline": ["warn", {
            "singleline": "never",
            "multiline": "never"
        }]

    }
}