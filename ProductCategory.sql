-- product_categories
CREATE TABLE product_categories (
  category_code  VARCHAR(20)  NOT NULL,
  status         VARCHAR(20)  NOT NULL DEFAULT 'active',
  created_at     TIMESTAMPTZ  NOT NULL DEFAULT (now() AT TIME ZONE 'utc'),
  created_by     VARCHAR(100) NOT NULL DEFAULT '',
  updated_at     TIMESTAMPTZ  NOT NULL DEFAULT (now() AT TIME ZONE 'utc'),
  updated_by     VARCHAR(100) NOT NULL DEFAULT '',
  CONSTRAINT pk_product_categories PRIMARY KEY (category_code)
);

-- product_category_translations
CREATE TABLE product_category_translations (
  category_code  VARCHAR(20)  NOT NULL,
  language_code  VARCHAR(10)  NOT NULL,
  category_name  VARCHAR(150) NOT NULL,
  CONSTRAINT pk_product_category_translations PRIMARY KEY (category_code, language_code),
  CONSTRAINT fk_pct_category FOREIGN KEY (category_code)
    REFERENCES product_categories (category_code) ON DELETE CASCADE
);
